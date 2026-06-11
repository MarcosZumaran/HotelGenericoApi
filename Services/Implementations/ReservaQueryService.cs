using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Constants;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Extensions;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class ReservaQueryService : IReservaQueryService
{
    private readonly HotelDbContext _db;

    public ReservaQueryService(HotelDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ReservaResponseDto>> GetPagedAsync(int page, int pageSize)
    {
        var query = _db.Reservas
            .Include(r => r.IdClienteNavigation)
            .Include(r => r.IdHabitacionNavigation)
            .AsNoTracking()
            .Select(r => new ReservaResponseDto(
                r.IdReserva,
                r.IdHabitacion,
                r.IdHabitacionNavigation != null ? r.IdHabitacionNavigation.NumeroHabitacion : null,
                r.IdClienteNavigation != null ? $"{r.IdClienteNavigation.Nombres} {r.IdClienteNavigation.Apellidos}" : null,
                r.FechaEntradaPrevista,
                r.FechaSalidaPrevista,
                r.MontoTotal,
                r.Estado ?? EstadoReservaCodigo.Code.Pendiente,
                r.IdClienteNavigation != null ? r.IdClienteNavigation.Documento : null,
                r.Observaciones,
                r.EsNoShow
            ));
        return await query.ToPagedResultAsync(page, pageSize);
    }

    public async Task<List<ReservaResponseDto>> GetAllAsync(string? estado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idHabitacion = null, string? cliente = null, string? tipo = null)
    {
        var query = _db.Reservas
            .Include(r => r.IdClienteNavigation)
            .Include(r => r.IdHabitacionNavigation)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(estado))
        {
            var estados = estado.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            query = query.Where(r => estados.Contains(r.IdEstadoReservaNavigation.Codigo));
        }

        if (fechaDesde.HasValue)
            query = query.Where(r => r.FechaEntradaPrevista.Date >= fechaDesde.Value.Date);

        if (fechaHasta.HasValue)
            query = query.Where(r => r.FechaSalidaPrevista.Date <= fechaHasta.Value.Date);

        if (idHabitacion.HasValue)
            query = query.Where(r => r.IdHabitacion == idHabitacion.Value);

        if (!string.IsNullOrEmpty(cliente))
            query = query.Where(r => (r.IdClienteNavigation.Nombres + " " + r.IdClienteNavigation.Apellidos).Contains(cliente));

        if (!string.IsNullOrEmpty(tipo))
        {
            if (tipo == "simple")
                query = query.Where(r => r.IdReservaCorporativa == null);
            else if (tipo == "multiple")
                query = query.Where(r => r.IdReservaCorporativa != null);
        }

        return await query
            .Select(r => new ReservaResponseDto(
                r.IdReserva,
                r.IdHabitacion,
                r.IdHabitacionNavigation != null ? r.IdHabitacionNavigation.NumeroHabitacion : null,
                r.IdClienteNavigation != null ? $"{r.IdClienteNavigation.Nombres} {r.IdClienteNavigation.Apellidos}" : null,
                r.FechaEntradaPrevista,
                r.FechaSalidaPrevista,
                r.MontoTotal,
                r.Estado ?? EstadoReservaCodigo.Code.Pendiente,
                r.IdClienteNavigation != null ? r.IdClienteNavigation.Documento : null,
                r.Observaciones,
                r.EsNoShow,
                r.IdReservaCorporativa
            ))
            .ToListAsync();
    }

    public async Task<List<FechaOcupadaDto>> GetFechasOcupadasAsync(int? idHabitacion = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        var query = _db.Reservas
            .Include(r => r.IdEstadoReservaNavigation)
            .AsNoTracking()
            .Where(r => r.IdEstadoReservaNavigation.Codigo == EstadoReservaCodigo.Code.Pendiente
                     || r.IdEstadoReservaNavigation.Codigo == EstadoReservaCodigo.Code.Confirmada);

        if (idHabitacion.HasValue)
            query = query.Where(r => r.IdHabitacion == idHabitacion.Value);

        if (fechaDesde.HasValue)
            query = query.Where(r => r.FechaSalidaPrevista >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(r => r.FechaEntradaPrevista <= fechaHasta.Value);

        return await query
            .Select(r => new FechaOcupadaDto(
                r.IdHabitacion,
                r.FechaEntradaPrevista,
                r.FechaSalidaPrevista
            ))
            .ToListAsync();
    }
}
