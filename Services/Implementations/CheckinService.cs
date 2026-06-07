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

    public CheckinService(
        HotelDbContext db,
        ILogger<CheckinService> logger,
        IHubContext<HabitacionHub> hubContext,
        IAmenidadService amenidadService,
        IReservaCorporativaService reservaCorporativaService)
    {
        _db = db;
        _logger = logger;
        _hubContext = hubContext;
        _amenidadService = amenidadService;
        _reservaCorporativaService = reservaCorporativaService;
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
            dto.Telefono, dto.IdClienteExistente, dto.GuardarCliente);

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
        string? telefono, int? idClienteExistente, bool guardarCliente)
    {
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

        return await _db.Clientes.FirstAsync(c => c.Documento == "00000000");
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
