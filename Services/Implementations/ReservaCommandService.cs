using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Constants;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class ReservaCommandService : IReservaCommandService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<ReservaCommandService> _logger;

    public ReservaCommandService(HotelDbContext db, ILogger<ReservaCommandService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Reserva> CreateReservaAsync(ReservaCreateDto dto, int idUsuario)
    {
        var habitacion = await _db.Habitaciones.FindAsync(dto.IdHabitacion)
            ?? throw new ArgumentException("Habitación no encontrada.");

        var cliente = await ResolverClienteAsync(
            dto.TipoDocumento, dto.Documento, dto.Nombres, dto.Apellidos,
            null, dto.IdClienteExistente, dto.GuardarCliente);

        var total = dto.EsPorHoras
            ? CalcularMontoPorHoras(dto.FechaEntradaPrevista, dto.FechaSalidaPrevista, habitacion.PrecioNoche)
            : CalcularMontoTotal(dto.FechaSalidaPrevista, habitacion.PrecioNoche);

        var reserva = new Reserva
        {
            IdCliente = cliente.IdCliente,
            IdHabitacion = dto.IdHabitacion,
            IdUsuario = idUsuario,
            FechaEntradaPrevista = dto.FechaEntradaPrevista,
            FechaSalidaPrevista = dto.FechaSalidaPrevista,
            MontoTotal = total,
            IdEstadoReserva = EstadoReservaCodigo.Confirmada,
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

        reserva.IdEstadoReserva = EstadoReservaCodigo.Cancelada;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Reserva {Id} cancelada", idReserva);
        return true;
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

    private static decimal CalcularMontoTotal(DateTime fechaSalida, decimal precioNoche)
    {
        var noches = Math.Max(1, (int)(fechaSalida.Date - DateTime.UtcNow.Date).TotalDays);
        return noches * precioNoche;
    }

    private static decimal CalcularMontoPorHoras(DateTime fechaEntrada, DateTime fechaSalida, decimal precioNoche)
    {
        var horas = Math.Max(1, (int)Math.Ceiling((fechaSalida - fechaEntrada).TotalHours));
        return Math.Round(horas * precioNoche / 24m * 0.75m, 2);
    }
}
