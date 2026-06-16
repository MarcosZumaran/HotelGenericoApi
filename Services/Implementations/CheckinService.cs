using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using HotelGenericoApi.Constants;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Hubs;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class CheckinService : ICheckinService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<CheckinService> _logger;
    private readonly IHubContext<HabitacionHub> _hubContext;
    private readonly IAmenidadService _amenidadService;
    private readonly IReservaCorporativaService _reservaCorporativaService;
    private readonly IParametroHotelService _parametroHotelService;

    public CheckinService(
        HotelDbContext db,
        ILogger<CheckinService> logger,
        IHubContext<HabitacionHub> hubContext,
        IAmenidadService amenidadService,
        IReservaCorporativaService reservaCorporativaService,
        IParametroHotelService parametroHotelService)
    {
        _db = db;
        _logger = logger;
        _hubContext = hubContext;
        _amenidadService = amenidadService;
        _reservaCorporativaService = reservaCorporativaService;
        _parametroHotelService = parametroHotelService;
    }

    public async Task<Estancia> CheckinAsync(CheckinCreateDto dto, int idUsuario)
    {
        var habitacion = await _db.Habitaciones
            .Include(h => h.IdEstadoNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == dto.IdHabitacion)
            ?? throw new ArgumentException("Habitación no encontrada.");

        if (habitacion.IdEstado != EstadoHabitacionCodigo.Disponible && habitacion.IdEstado != EstadoHabitacionCodigo.Reservada)
            throw new InvalidOperationException($"La habitación {habitacion.NumeroHabitacion} no está disponible.");

        if (dto.IdReservaCorporativa.HasValue)
        {
            var puedeAsignar = await _reservaCorporativaService.ValidarYAsignarHabitacionAsync(dto.IdReservaCorporativa.Value);
            if (!puedeAsignar)
                throw new InvalidOperationException("La reserva corporativa ya alcanzó el número máximo de habitaciones.");
        }

        var cliente = await ResolverClienteAsync(
            dto.TipoDocumento, dto.Documento, dto.Nombres, dto.Apellidos,
            dto.Telefono, dto.IdClienteExistente, dto.GuardarCliente, dto.UsarClienteAnonimo);

        var total = CalcularMontoTotal(
            dto.FechaCheckoutPrevista, habitacion.PrecioNoche, dto.EsPorHoras);

        var estancia = new Estancia
        {
            IdReserva = dto.IdReserva,
            IdHabitacion = dto.IdHabitacion,
            IdClienteTitular = cliente.IdCliente,
            FechaCheckin = DateTime.UtcNow,
            FechaCheckoutPrevista = dto.FechaCheckoutPrevista,
            MontoTotal = total,
            IdEstadoEstancia = EstadoEstanciaCodigo.Activa,
            IdReservaCorporativa = dto.IdReservaCorporativa,
            MetodoPago = dto.MetodoPago
        };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Estancias.Add(estancia);
            await _db.SaveChangesAsync();

            var pago = new Pago
            {
                IdEstancia = estancia.IdEstancia,
                Monto = estancia.MontoTotal,
                MetodoPago = estancia.MetodoPago ?? MetodoPagoCodigo.Efectivo,
                FechaPago = DateTime.UtcNow,
                Concepto = "Habitación"
            };
            _db.Pagos.Add(pago);

            var depParams = await _parametroHotelService.GetDepositoGarantiaParamsAsync();
            if (bool.TryParse(depParams.DepositoHabilitado, out var depHabilitado) && depHabilitado && dto.AplicarDepositoGarantia)
            {
                decimal.TryParse(depParams.DepositoMonto, out var depMonto);
                decimal.TryParse(depParams.DepositoPorcentaje, out var depPorcentaje);

                var depositoCalculado = Math.Max(depMonto, estancia.MontoTotal * depPorcentaje / 100m);
                if (depositoCalculado > 0)
                {
                    var pagoDeposito = new Pago
                    {
                        IdEstancia = estancia.IdEstancia,
                        Monto = depositoCalculado,
                        MetodoPago = estancia.MetodoPago ?? MetodoPagoCodigo.Efectivo,
                        FechaPago = DateTime.UtcNow,
                        Concepto = "Depósito de garantía"
                    };
                    _db.Pagos.Add(pagoDeposito);
                }
            }

            var earlyParams = await _parametroHotelService.GetEarlyCheckinParamsAsync();
            if (TimeSpan.TryParse(earlyParams.EarlyCheckinHoraLimite, out var earlyHora)
                && decimal.TryParse(earlyParams.EarlyCheckinCargo, out var earlyCargo)
                && earlyCargo > 0)
            {
                var ahora = DateTime.UtcNow;
                var horaEntradaNormal = ahora.Date.Add(earlyHora);
                if (ahora < horaEntradaNormal)
                {
                    var earlyProduct = await _db.Productos.FirstOrDefaultAsync(p => p.Nombre == "Early check-in");
                    if (earlyProduct == null)
                    {
                        earlyProduct = new Producto
                        {
                            Nombre = "Early check-in",
                            Descripcion = "Cargo por entrada anticipada",
                            PrecioUnitario = earlyCargo,
                            IdAfectacionIgv = "10",
                            Stock = 0,
                            StockMinimo = 0,
                            UnidadMedida = "NIU",
                            EsAmenidad = false,
                            EsVendibleEnTienda = false,
                            CreatedAt = DateTime.UtcNow,
                        };
                        _db.Productos.Add(earlyProduct);
                        await _db.SaveChangesAsync();
                    }

                    _db.ItemsEstancia.Add(new ItemEstancia
                    {
                        IdEstancia = estancia.IdEstancia,
                        IdProducto = earlyProduct.IdProducto,
                        Cantidad = 1,
                        PrecioUnitario = earlyCargo,
                        Subtotal = earlyCargo,
                        FechaRegistro = DateTime.UtcNow,
                    });
                }
            }

            habitacion.IdEstado = EstadoHabitacionCodigo.Ocupada;
            habitacion.FechaUltimoCambio = DateTime.UtcNow;

            if (dto.IdReserva.HasValue)
            {
                var reserva = await _db.Reservas.FindAsync(dto.IdReserva.Value);
                if (reserva != null)
                {
                    reserva.IdEstadoReserva = EstadoReservaCodigo.Completa;
                    reserva.EsNoShow = false;
                }
            }

            if (dto.IdReservaCorporativa.HasValue)
            {
                var corporativa = await _db.ReservasCorporativas.FindAsync(dto.IdReservaCorporativa.Value);
                if (corporativa != null && corporativa.Estado == EstadoReservaCodigo.Code.Pendiente)
                {
                    corporativa.Estado = EstadoReservaCodigo.Code.Confirmada;
                }
            }

            if (dto.Huespedes?.Count > 0)
            {
                foreach (var h in dto.Huespedes)
                {
                    var c = await CrearObtenerClienteAsync(h.TipoDocumento, h.Documento, h.Nombres, h.Apellidos, h.Telefono, h.EsAnonimo);
                    _db.Huespedes.Add(new Huesped
                    {
                        IdEstancia = estancia.IdEstancia,
                        IdCliente = c.IdCliente,
                        EsTitular = h.EsTitular,
                        FechaRegistro = DateTime.UtcNow,
                    });
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

    private async Task<Cliente> ResolverClienteAsync(
        string tipoDocumento, string documento, string nombres, string apellidos,
        string? telefono, int? idClienteExistente, bool guardarCliente, bool usarClienteAnonimo)
    {
        if (usarClienteAnonimo)
            return await ObtenerClienteAnonimoAsync();

        tipoDocumento = TipoDocumentoMapper.Normalize(tipoDocumento);
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

        return await ObtenerClienteAnonimoAsync();
    }

    private async Task<Cliente> CrearObtenerClienteAsync(
        string tipoDocumento, string documento, string nombres, string apellidos,
        string? telefono, bool esAnonimo)
    {
        if (esAnonimo)
            return await ObtenerClienteAnonimoAsync();

        tipoDocumento = TipoDocumentoMapper.Normalize(tipoDocumento);

        if (!string.IsNullOrWhiteSpace(documento))
        {
            var existente = await _db.Clientes
                .FirstOrDefaultAsync(c => c.TipoDocumento == tipoDocumento && c.Documento == documento);
            if (existente != null) return existente;
        }

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

    private async Task<Cliente> ObtenerClienteAnonimoAsync()
    {
        var anonimo = await _db.Clientes.FirstOrDefaultAsync(c => c.Documento == "00000000");
        if (anonimo != null)
            return anonimo;

        anonimo = new Cliente
        {
            TipoDocumento = "0",
            Documento = "00000000",
            Nombres = "Anonimo",
            Apellidos = "",
            Nacionalidad = "PERUANA"
        };
        _db.Clientes.Add(anonimo);
        await _db.SaveChangesAsync();
        return anonimo;
    }

    internal static decimal CalcularMontoTotal(DateTime fechaSalida, decimal precioNoche, bool esPorHoras)
    {
        if (esPorHoras)
        {
            var horas = Math.Max(1, (int)(fechaSalida - DateTime.UtcNow).TotalHours);
            var bloques = (int)Math.Ceiling(horas / 3.0);
            return bloques * 20.0m;
        }
        var noches = Math.Max(1, (int)(fechaSalida.Date - DateTime.UtcNow.Date).TotalDays);
        return noches * precioNoche;
    }
}
