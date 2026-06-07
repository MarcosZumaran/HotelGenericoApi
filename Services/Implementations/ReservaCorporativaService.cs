using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Constants;
using HotelGenericoApi.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using HotelGenericoApi.Hubs;

namespace HotelGenericoApi.Services.Implementations;

public class ReservaCorporativaService : IReservaCorporativaService
{
    private readonly HotelDbContext _db;
    private readonly IHubContext<HabitacionHub> _hubContext;

    public ReservaCorporativaService(HotelDbContext db, IHubContext<HabitacionHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    public async Task<IEnumerable<ReservaCorporativaResponseDto>> GetAllAsync()
    {
        var reservas = await _db.ReservasCorporativas
            .Include(r => r.IdClienteEmpresaNavigation)
            .Include(r => r.Estancias)
                .ThenInclude(e => e.ItemsEstancia)
            .AsNoTracking()
            .ToListAsync();

        var result = reservas.Select(r => new ReservaCorporativaResponseDto
        {
            IdReservaCorporativa = r.IdReservaCorporativa,
            IdClienteEmpresa = r.IdClienteEmpresa,
            NombreEmpresa = r.ClienteEmpresa != null ? $"{r.ClienteEmpresa.Nombres} {r.ClienteEmpresa.Apellidos}" : "",
            RucEmpresa = r.ClienteEmpresa != null ? r.ClienteEmpresa.Documento : "",
            FechaInicio = r.FechaInicio.ToDateTime(TimeOnly.MinValue),
            FechaFin = r.FechaFin.ToDateTime(TimeOnly.MinValue),
            NumeroHabitaciones = r.NumeroHabitaciones,
            HabitacionesOcupadas = r.Estancias.Count(e => e.Estado == EstadoEstanciaCodigo.Code.Activa),
            Estado = r.Estado,
            TotalAcumulado = r.Estancias
                .Where(e => e.FechaCheckoutReal != null)
                .Sum(e => e.MontoTotal + (e.ItemsEstancia != null ? e.ItemsEstancia.Sum(i => i.Subtotal.GetValueOrDefault()) : 0)),
            Observaciones = r.Observaciones,
            FechaRegistro = r.FechaRegistro
        });

        return result;
    }

    public async Task<ReservaCorporativaResponseDto?> GetByIdAsync(int id)
    {
        var reserva = await _db.ReservasCorporativas
            .Include(r => r.IdClienteEmpresaNavigation)
            .Include(r => r.Estancias)
                .ThenInclude(e => e.ItemsEstancia)
            .FirstOrDefaultAsync(r => r.IdReservaCorporativa == id);

        if (reserva == null) return null;

        var reservasIndividuales = await _db.Reservas
            .Where(r => r.IdReservaCorporativa == id)
            .Include(r => r.IdHabitacionNavigation)
                .ThenInclude(h => h.IdTipoNavigation)
            .ToListAsync();

        var estanciasActivas = await _db.Estancias
            .Where(e => e.IdReservaCorporativa == id && e.IdEstadoEstanciaNavigation.Codigo == EstadoEstanciaCodigo.Code.Activa)
            .ToListAsync();

        var habitacionesDto = reservasIndividuales.Select(r => new HabitacionResumenDto
        {
            IdHabitacion = r.IdHabitacion,
            NumeroHabitacion = r.Habitacion?.NumeroHabitacion ?? "",
            TipoNombre = r.Habitacion?.Tipo?.Nombre ?? "",
            PrecioNoche = r.Habitacion?.PrecioNoche ?? 0,
            IdReserva = r.IdReserva,
            Estado = "Reservada",
            IdEstanciaActiva = null
        }).ToList();

        foreach (var est in estanciasActivas)
        {
            var hab = habitacionesDto.FirstOrDefault(h => h.IdHabitacion == est.IdHabitacion);
            if (hab != null)
            {
                hab.Estado = "Ocupada";
                hab.IdEstanciaActiva = est.IdEstancia;
            }
        }

        return new ReservaCorporativaResponseDto
        {
            IdReservaCorporativa = reserva.IdReservaCorporativa,
            IdClienteEmpresa = reserva.IdClienteEmpresa,
            NombreEmpresa = reserva.ClienteEmpresa != null ? $"{reserva.ClienteEmpresa.Nombres} {reserva.ClienteEmpresa.Apellidos}" : "",
            RucEmpresa = reserva.ClienteEmpresa != null ? reserva.ClienteEmpresa.Documento : "",
            FechaInicio = reserva.FechaInicio.ToDateTime(TimeOnly.MinValue),
            FechaFin = reserva.FechaFin.ToDateTime(TimeOnly.MinValue),
            NumeroHabitaciones = reserva.NumeroHabitaciones,
            HabitacionesOcupadas = estanciasActivas.Count,
            Estado = reserva.Estado,
            TotalAcumulado = reserva.Estancias
                .Where(e => e.FechaCheckoutReal != null)
                .Sum(e => e.MontoTotal + (e.ItemsEstancia != null ? e.ItemsEstancia.Sum(i => i.Subtotal.GetValueOrDefault()) : 0)),
            Observaciones = reserva.Observaciones,
            FechaRegistro = reserva.FechaRegistro,
            Habitaciones = habitacionesDto
        };
    }

    public async Task<ReservaCorporativaResponseDto> CreateAsync(ReservaCorporativaCreateDto dto, int idUsuario)
    {
        var cliente = await _db.Clientes.FindAsync(dto.IdClienteEmpresa);
        if (cliente == null)
            throw new ArgumentException("Cliente no encontrado.");

        var reservaMultiple = new ReservaCorporativa
        {
            IdClienteEmpresa = dto.IdClienteEmpresa,
            FechaInicio = DateOnly.FromDateTime(dto.FechaInicio),
            FechaFin = DateOnly.FromDateTime(dto.FechaFin),
            NumeroHabitaciones = dto.HabitacionesIds.Count,
            Estado = EstadoReservaCodigo.Code.Confirmada,
            Observaciones = dto.Observaciones,
            FechaRegistro = DateTime.UtcNow
        };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.ReservasCorporativas.Add(reservaMultiple);
            await _db.SaveChangesAsync();

            foreach (var habId in dto.HabitacionesIds)
            {
                var habitacion = await _db.Habitaciones.FindAsync(habId);
                if (habitacion == null) continue;

                var noches = Math.Max(1, (int)(dto.FechaFin - dto.FechaInicio).TotalDays);
                var montoTotal = noches * habitacion.PrecioNoche;

                var reserva = new Reserva
                {
                    IdCliente = dto.IdClienteEmpresa,
                    IdHabitacion = habId,
                    IdUsuario = idUsuario,
                    FechaRegistro = DateTime.UtcNow,
                    FechaEntradaPrevista = dto.FechaInicio,
                    FechaSalidaPrevista = dto.FechaFin,
                    MontoTotal = montoTotal,
                    IdEstadoReserva = EstadoReservaCodigo.Confirmada,
                    EsNoShow = false,
                    Observaciones = $"Reserva múltiple #{reservaMultiple.IdReservaCorporativa}",
                    IdReservaCorporativa = reservaMultiple.IdReservaCorporativa
                };
                _db.Reservas.Add(reserva);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            foreach (var habId in dto.HabitacionesIds)
            {
                await _hubContext.Clients.All.SendAsync("ReservaCreada", new { idHabitacion = habId });
            }

            return (await GetByIdAsync(reservaMultiple.IdReservaCorporativa))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, ReservaCorporativaCreateDto dto, int idUsuario)
    {
        var reserva = await _db.ReservasCorporativas
            .Include(r => r.Estancias)
            .FirstOrDefaultAsync(r => r.IdReservaCorporativa == id);
        if (reserva == null) return false;

        if (reserva.Estado != EstadoReservaCodigo.Code.Pendiente)
            throw new InvalidOperationException("Solo se pueden modificar reservas en estado Pendiente.");

        reserva.FechaInicio = DateOnly.FromDateTime(dto.FechaInicio);
        reserva.FechaFin = DateOnly.FromDateTime(dto.FechaFin);
        reserva.NumeroHabitaciones = dto.HabitacionesIds.Count;
        reserva.Observaciones = dto.Observaciones;

        // Eliminar reservas individuales antiguas
        var reservasExistentes = await _db.Reservas
            .Where(r => r.IdReservaCorporativa == id)
            .ToListAsync();
        _db.Reservas.RemoveRange(reservasExistentes);

        // Crear nuevas reservas individuales con las habitaciones actualizadas
        foreach (var habId in dto.HabitacionesIds)
        {
            var habitacion = await _db.Habitaciones.FindAsync(habId);
            if (habitacion == null) continue;

            var noches = Math.Max(1, (int)(dto.FechaFin - dto.FechaInicio).TotalDays);
            var montoTotal = noches * habitacion.PrecioNoche;

            var nuevaReserva = new Reserva
            {
                IdCliente = dto.IdClienteEmpresa,
                IdHabitacion = habId,
                IdUsuario = idUsuario,
                FechaRegistro = DateTime.UtcNow,
                FechaEntradaPrevista = dto.FechaInicio,
                FechaSalidaPrevista = dto.FechaFin,
                MontoTotal = montoTotal,
                IdEstadoReserva = EstadoReservaCodigo.Confirmada,
                EsNoShow = false,
                Observaciones = $"Reserva múltiple #{reserva.IdReservaCorporativa} (actualizada)",
                IdReservaCorporativa = reserva.IdReservaCorporativa
            };
            _db.Reservas.Add(nuevaReserva);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var reserva = await _db.ReservasCorporativas
            .Include(r => r.Estancias)
            .FirstOrDefaultAsync(r => r.IdReservaCorporativa == id);

        if (reserva == null) return false;

        if (reserva.Estancias.Any(e => e.Estado == EstadoEstanciaCodigo.Code.Activa))
            throw new InvalidOperationException("No se puede eliminar una reserva corporativa con estancias activas.");

        if (reserva.Estado != EstadoReservaCodigo.Code.Pendiente)
            throw new InvalidOperationException("Solo se pueden eliminar reservas en estado Pendiente.");

        var reservasIndividuales = await _db.Reservas
            .Where(r => r.IdReservaCorporativa == id)
            .ToListAsync();
        _db.Reservas.RemoveRange(reservasIndividuales);

        _db.ReservasCorporativas.Remove(reserva);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ReservaCorporativaResponseDto> FinalizarAsync(int id, int idUsuario)
    {
        var reserva = await _db.ReservasCorporativas
            .Include(r => r.Estancias)
                .ThenInclude(e => e.ItemsEstancia)
            .Include(r => r.IdClienteEmpresaNavigation)
            .FirstOrDefaultAsync(r => r.IdReservaCorporativa == id);

        if (reserva == null)
            throw new ArgumentException("Reserva corporativa no encontrada.");

        if (reserva.Estado == EstadoEstanciaCodigo.Code.Finalizada)
            throw new InvalidOperationException("La reserva ya está finalizada.");

        var estanciasPendientes = reserva.Estancias.Any(e => e.FechaCheckoutReal == null);
        if (estanciasPendientes)
            throw new InvalidOperationException("No se puede finalizar porque hay estancias activas sin checkout.");

        decimal totalFinal = reserva.Estancias.Sum(e => e.MontoTotal + (e.ItemsEstancia != null ? e.ItemsEstancia.Sum(i => i.Subtotal.GetValueOrDefault()) : 0));

        var comprobante = new Comprobante
        {
            IdEstancia = null,
            IdVenta = null,
            TipoComprobante = "01",
            Serie = "F001",
            Correlativo = await ObtenerSiguienteCorrelativo("F001"),
            FechaEmision = DateTime.UtcNow,
            MontoTotal = totalFinal,
            IgvMonto = totalFinal * 0.18m,
            ClienteDocumentoTipo = reserva.ClienteEmpresa!.TipoDocumento,
            ClienteDocumentoNum = reserva.ClienteEmpresa.Documento,
            ClienteNombre = $"{reserva.ClienteEmpresa.Nombres} {reserva.ClienteEmpresa.Apellidos}",
            MetodoPago = null,
            IdEstadoSunat = 1,
            HashXml = null
        };

        _db.Comprobantes.Add(comprobante);
        reserva.Estado = EstadoEstanciaCodigo.Code.Finalizada;
        await _db.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> ValidarYAsignarHabitacionAsync(int idReservaCorporativa)
    {
        var reserva = await _db.ReservasCorporativas
            .Include(r => r.Estancias)
            .FirstOrDefaultAsync(r => r.IdReservaCorporativa == idReservaCorporativa);

        if (reserva == null) return false;
        if (reserva.Estado != EstadoReservaCodigo.Code.Confirmada && reserva.Estado != EstadoReservaCodigo.Code.Pendiente) return false;

        var activas = reserva.Estancias.Count(e => e.Estado == EstadoEstanciaCodigo.Code.Activa);
        return activas < reserva.NumeroHabitaciones;
    }

    private async Task<int> ObtenerSiguienteCorrelativo(string serie)
    {
        var ultimo = await _db.Comprobantes
            .Where(c => c.Serie == serie)
            .MaxAsync(c => (int?)c.Correlativo) ?? 0;
        return ultimo + 1;
    }
}
