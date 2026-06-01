using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Hubs;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class EstanciaService : IEstanciaService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<EstanciaService> _logger;
    private readonly IHubContext<HabitacionHub> _hubContext;

    private readonly IAmenidadService _amenidadService;

    public EstanciaService(HotelDbContext db, ILogger<EstanciaService> logger, IHubContext<HabitacionHub> hubContext, IAmenidadService amenidadService)
    {
        _db = db;
        _logger = logger;
        _hubContext = hubContext;
        _amenidadService = amenidadService;
    }

    // CONSULTAS

    public async Task<List<Estancia>> GetAllAsync()
    {
        return await _db.Estancias
            .Include(e => e.Habitacion!).ThenInclude(h => h.Tipo)
            .Include(e => e.ClienteTitular)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Estancia?> GetByIdAsync(int id)
    {
        return await _db.Estancias
            .Include(e => e.Habitacion!).ThenInclude(h => h.Tipo)
            .Include(e => e.ClienteTitular)
            .Include(e => e.ItemsEstancia!).ThenInclude(i => i.Producto)
            .Include(e => e.Huespedes!).ThenInclude(h => h.Cliente)
            .FirstOrDefaultAsync(e => e.IdEstancia == id);
    }

    public async Task<List<ItemConsumoResponseDto>> GetConsumosAsync(int idEstancia)
    {
        return await _db.ItemsEstancia
            .Where(i => i.IdEstancia == idEstancia)
            .Include(i => i.Producto)
            .Select(i => new ItemConsumoResponseDto(
                i.IdItem,
                i.IdProducto,
                i.Producto!.Nombre,
                i.Cantidad,
                i.PrecioUnitario,
                i.Cantidad * i.PrecioUnitario,
                i.FechaRegistro
            ))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ReservaResponseDto>> GetReservasByHabitacionAsync(int idHabitacion)
    {
        return await _db.Reservas
            .Where(r => r.IdHabitacion == idHabitacion)
            .Include(r => r.Cliente)
            .Select(r => new ReservaResponseDto(
                r.IdReserva,
                r.IdHabitacion,
                r.Habitacion!.NumeroHabitacion,
                r.Cliente!.Nombres + " " + r.Cliente.Apellidos,
                r.FechaEntradaPrevista,
                r.FechaSalidaPrevista,
                r.MontoTotal,
                r.Estado ?? "Pendiente",
                r.Cliente.Documento,
                r.Observaciones,
                r.EsNoShow
            ))
            .AsNoTracking()
            .ToListAsync();
    }

    // OPERACIONES PRINCIPALES

    public async Task<Estancia> CheckinAsync(CheckinCreateDto dto, int idUsuario)
    {
        var habitacion = await _db.Habitaciones
            .Include(h => h.Estado)
            .FirstOrDefaultAsync(h => h.IdHabitacion == dto.IdHabitacion)
            ?? throw new ArgumentException("Habitación no encontrada.");

        if (habitacion.IdEstado != 1 && habitacion.IdEstado != 5)
            throw new InvalidOperationException($"La habitación {habitacion.NumeroHabitacion} no está disponible.");

        var cliente = await ResolverClienteAsync(dto.TipoDocumento, dto.Documento, dto.Nombres, dto.Apellidos, dto.Telefono, dto.IdClienteExistente, dto.GuardarCliente);
        var total = CalcularMontoTotal(dto.FechaCheckoutPrevista, habitacion.PrecioNoche);

        var estancia = new Estancia
        {
            IdReserva = dto.IdReserva,
            IdHabitacion = dto.IdHabitacion,
            IdClienteTitular = cliente.IdCliente,
            FechaCheckin = DateTime.UtcNow,
            FechaCheckoutPrevista = dto.FechaCheckoutPrevista,
            MontoTotal = total,
            Estado = "Activa",
        };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Estancias.Add(estancia);
            await _db.SaveChangesAsync();

            habitacion.IdEstado = 2;
            habitacion.FechaUltimoCambio = DateTime.UtcNow;

            if (dto.IdReserva.HasValue)
            {
                var reserva = await _db.Reservas.FindAsync(dto.IdReserva.Value);
                if (reserva != null)
                {
                    reserva.Estado = "Completa";
                    reserva.EsNoShow = false;
                }
            }

            _logger.LogInformation("Check-in realizado: Estancia {Id}, Habitación {Numero}", estancia.IdEstancia, habitacion.NumeroHabitacion);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            await _hubContext.Clients.All.SendAsync("NuevaEstancia", new
            {
                idEstancia = estancia.IdEstancia,
                idHabitacion = estancia.IdHabitacion,
                numeroHabitacion = habitacion.NumeroHabitacion,
                cliente = $"{cliente.Nombres} {cliente.Apellidos}"
            });

            await _amenidadService.InicializarStockHabitacionAsync(estancia.IdHabitacion);

            return estancia;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<CheckoutResultDto> RealizarCheckoutAsync(int estanciaId, int idUsuario)
    {
        var estancia = await _db.Estancias
            .Include(e => e.Habitacion)
            .Include(e => e.ItemsEstancia)
            .FirstOrDefaultAsync(e => e.IdEstancia == estanciaId);

        if (estancia == null)
            throw new ArgumentException("Estancia no encontrada.");
        if (estancia.FechaCheckoutReal != null)
            throw new InvalidOperationException("La estancia ya tiene checkout realizado.");

        decimal totalConsumos = estancia.ItemsEstancia?.Sum(i => i.Subtotal) ?? 0;
        decimal totalHabitacion = estancia.MontoTotal;
        decimal totalFinal = totalHabitacion + totalConsumos;

        var clienteTitular = await _db.Clientes.FindAsync(estancia.IdClienteTitular)
            ?? throw new Exception("Cliente titular no encontrado.");

        string tipoComprobante = (clienteTitular.TipoDocumento == "6") ? "01" : "03";
        string serie = (tipoComprobante == "01") ? "F001" : "B001";
        int correlativo = await ObtenerSiguienteCorrelativo(serie);
        decimal igv = totalFinal * 0.18m;

        var comprobante = new Comprobante
        {
            IdEstancia = estanciaId,
            TipoComprobante = tipoComprobante,
            Serie = serie,
            Correlativo = correlativo,
            FechaEmision = DateTime.UtcNow,
            MontoTotal = totalFinal,
            IgvMonto = igv,
            ClienteDocumentoTipo = clienteTitular.TipoDocumento,
            ClienteDocumentoNum = clienteTitular.Documento,
            ClienteNombre = $"{clienteTitular.Nombres} {clienteTitular.Apellidos}",
            MetodoPago = null,
            IdEstadoSunat = 1,
            HashXml = null
        };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Comprobantes.Add(comprobante);
            await _db.SaveChangesAsync();

            estancia.FechaCheckoutReal = DateTime.UtcNow;
            estancia.Estado = "Finalizada";

            if (estancia.Habitacion != null)
            {
                estancia.Habitacion.IdEstado = 3;
                estancia.Habitacion.FechaUltimoCambio = DateTime.UtcNow;
                estancia.Habitacion.UsuarioCambio = idUsuario;

                _db.HistorialEstadoHabitaciones.Add(new HistorialEstadoHabitacion
                {
                    IdHabitacion = estancia.Habitacion.IdHabitacion,
                    IdEstadoAnterior = 2,
                    IdEstadoNuevo = 3,
                    FechaCambio = DateTime.UtcNow,
                    IdUsuario = idUsuario
                });
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            await _hubContext.Clients.All.SendAsync("EstadoHabitacionCambiado", new
            {
                idHabitacion = estancia.IdHabitacion,
                numero = estancia.Habitacion?.NumeroHabitacion,
                nuevoEstado = "Limpieza"
            });

            _logger.LogInformation("Checkout realizado para estancia {Id}. Total: {Total}, Comprobante: {Serie}-{Correlativo}",
                estanciaId, totalFinal, serie, correlativo);

            return new CheckoutResultDto
            {
                TotalHabitacion = totalHabitacion,
                TotalConsumos = totalConsumos,
                TotalFinal = totalFinal,
                ComprobanteId = comprobante.IdComprobante
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // SALIDAS TEMPORALES

    public async Task RegistrarSalidaTemporalAsync(int estanciaId, bool llavesDejadas)
    {
        var estancia = await _db.Estancias.FindAsync(estanciaId);
        if (estancia == null)
            throw new ArgumentException("Estancia no encontrada.");
        if (estancia.EstaFuera)
            throw new InvalidOperationException("El huésped ya está marcado como fuera.");

        estancia.EstaFuera = true;
        estancia.HoraSalidaTemporal = DateTime.UtcNow;
        estancia.LlavesDejadas = llavesDejadas;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Salida temporal registrada para estancia {Id}, llaves dejadas: {Llaves}", estanciaId, llavesDejadas);
    }

    public async Task RegistrarRegresoAsync(int estanciaId)
    {
        var estancia = await _db.Estancias.FindAsync(estanciaId);
        if (estancia == null)
            throw new ArgumentException("Estancia no encontrada.");
        if (!estancia.EstaFuera)
            throw new InvalidOperationException("El huésped no estaba fuera.");

        estancia.EstaFuera = false;
        estancia.HoraRegresoTemporal = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Regreso registrado para estancia {Id}", estanciaId);
    }

    // HUÉSPEDES ADICIONALES

    public async Task<Huesped> AgregarHuespedCompletoAsync(int estanciaId, AgregarHuespedDto dto)
    {
        var estancia = await _db.Estancias.FindAsync(estanciaId);
        if (estancia == null)
            throw new ArgumentException("Estancia no encontrada.");

        var cliente = await _db.Clientes
            .FirstOrDefaultAsync(c => c.TipoDocumento == dto.TipoDocumento && c.Documento == dto.Documento);
        if (cliente == null)
        {
            cliente = new Cliente
            {
                TipoDocumento = dto.TipoDocumento,
                Documento = dto.Documento,
                Nombres = dto.Nombres,
                Apellidos = dto.Apellidos,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Nacionalidad = "PERUANA",
                FechaRegistro = DateTime.UtcNow
            };
            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync();
        }

        var yaExiste = await _db.Huespedes
            .AnyAsync(h => h.IdEstancia == estanciaId && h.IdCliente == cliente.IdCliente);
        if (yaExiste)
            throw new InvalidOperationException("El huésped ya está registrado en esta estancia.");

        var huesped = new Huesped
        {
            IdEstancia = estanciaId,
            IdCliente = cliente.IdCliente,
            EsTitular = false,
            FechaRegistro = DateTime.UtcNow
        };
        _db.Huespedes.Add(huesped);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Huésped {ClienteId} agregado a estancia {EstanciaId}", cliente.IdCliente, estanciaId);
        return huesped;
    }

    // CONSUMOS

    public async Task<bool> AddConsumoAsync(int idEstancia, ItemEstancia item)
    {
        var estancia = await _db.Estancias.FindAsync(idEstancia);
        if (estancia == null) return false;

        var producto = await _db.Productos.FindAsync(item.IdProducto);
        if (producto == null) return false;

        if (producto.Stock < item.Cantidad)
            throw new InvalidOperationException($"Stock insuficiente para {producto.Nombre}");

        item.IdEstancia = idEstancia;
        item.Subtotal = item.Cantidad * item.PrecioUnitario;
        item.FechaRegistro = DateTime.UtcNow;

        producto.Stock -= item.Cantidad;

        _db.ItemsEstancia.Add(item);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Consumo añadido a estancia {IdEstancia}: Producto {IdProducto}, Cantidad {Cantidad}", idEstancia, item.IdProducto, item.Cantidad);
        return true;
    }

    public async Task<bool> UpdateConsumoAsync(int idItem, int cantidad)
    {
        var item = await _db.ItemsEstancia.FindAsync(idItem);
        if (item == null) return false;

        item.Cantidad = cantidad;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Consumo {IdItem} actualizado a cantidad {Cantidad}", idItem, cantidad);
        return true;
    }

    public async Task<bool> DeleteConsumoAsync(int idItem)
    {
        var item = await _db.ItemsEstancia.FindAsync(idItem);
        if (item == null) return false;

        _db.ItemsEstancia.Remove(item);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Consumo {IdItem} eliminado", idItem);
        return true;
    }

    // RESERVAS

    public async Task<Reserva> CreateReservaAsync(ReservaCreateDto dto, int idUsuario)
    {
        var habitacion = await _db.Habitaciones.FindAsync(dto.IdHabitacion)
            ?? throw new ArgumentException("Habitación no encontrada.");

        var cliente = await ResolverClienteAsync(dto.TipoDocumento, dto.Documento, dto.Nombres, dto.Apellidos, null, dto.IdClienteExistente, dto.GuardarCliente);
        var total = CalcularMontoTotal(dto.FechaSalidaPrevista, habitacion.PrecioNoche);

        var reserva = new Reserva
        {
            IdCliente = cliente.IdCliente,
            IdHabitacion = dto.IdHabitacion,
            IdUsuario = idUsuario,
            FechaEntradaPrevista = dto.FechaEntradaPrevista,
            FechaSalidaPrevista = dto.FechaSalidaPrevista,
            MontoTotal = total,
            Estado = "Confirmada",
            EsNoShow = false,
        };

        _db.Reservas.Add(reserva);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Reserva {Id} creada para habitación {Numero}", reserva.IdReserva, habitacion.NumeroHabitacion);
        return reserva;
    }

    public async Task<bool> CancelarReservaAsync(int idReserva)
    {
        var reserva = await _db.Reservas.FindAsync(idReserva);
        if (reserva == null) return false;

        reserva.Estado = "Cancelada";
        await _db.SaveChangesAsync();
        _logger.LogInformation("Reserva {Id} cancelada", idReserva);
        return true;
    }

    // MÉTODOS PRIVADOS

    private async Task<Cliente> ResolverClienteAsync(string tipoDocumento, string documento, string nombres, string apellidos, string? telefono, int? idClienteExistente, bool guardarCliente)
    {
        if (idClienteExistente.HasValue)
        {
            return await _db.Clientes.FindAsync(idClienteExistente.Value)
                ?? throw new ArgumentException("Cliente existente no encontrado.");
        }

        if (!string.IsNullOrWhiteSpace(documento) && !string.IsNullOrWhiteSpace(nombres) && guardarCliente)
        {
            var existente = await _db.Clientes
                .FirstOrDefaultAsync(c => c.TipoDocumento == tipoDocumento && c.Documento == documento);
            if (existente != null) return existente;

            var nuevo = new Cliente
            {
                TipoDocumento = tipoDocumento,
                Documento = documento,
                Nombres = nombres,
                Apellidos = apellidos,
                Telefono = telefono,
                Nacionalidad = "PERUANA"
            };
            _db.Clientes.Add(nuevo);
            await _db.SaveChangesAsync();
            return nuevo;
        }

        return await _db.Clientes.FirstAsync(c => c.Documento == "00000000");
    }

    private static decimal CalcularMontoTotal(DateTime fechaSalida, decimal precioNoche)
    {
        var noches = Math.Max(1, (int)(fechaSalida.Date - DateTime.UtcNow.Date).TotalDays);
        return noches * precioNoche;
    }

    private async Task<int> ObtenerSiguienteCorrelativo(string serie)
    {
        var ultimo = await _db.Comprobantes
            .Where(c => c.Serie == serie)
            .MaxAsync(c => (int?)c.Correlativo) ?? 0;
        return ultimo + 1;
    }
}
