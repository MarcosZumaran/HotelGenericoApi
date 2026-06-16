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
    private readonly ILogger<ReservaQueryService> _logger;

    public ReservaQueryService(HotelDbContext db, ILogger<ReservaQueryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<ReservaResponseDto>> GetPagedAsync(int page, int pageSize)
    {
        await MarcarReservasVencidasAsync();

        var query = _db.Reservas
            .Include(r => r.IdClienteNavigation)
            .Include(r => r.IdHabitacionNavigation)
            .Include(r => r.IdEstadoReservaNavigation)
            .AsNoTracking()
            .Select(r => new ReservaResponseDto(
                r.IdReserva,
                r.IdHabitacion,
                r.IdHabitacionNavigation != null ? r.IdHabitacionNavigation.NumeroHabitacion : null,
                r.IdClienteNavigation != null ? $"{r.IdClienteNavigation.Nombres} {r.IdClienteNavigation.Apellidos}" : null,
                r.FechaEntradaPrevista,
                r.FechaSalidaPrevista,
                r.MontoTotal,
                r.IdEstadoReservaNavigation.Codigo ?? EstadoReservaCodigo.Code.Pendiente,
                r.IdClienteNavigation != null ? r.IdClienteNavigation.Documento : null,
                r.Observaciones,
                r.EsNoShow
            ));
        return await query.ToPagedResultAsync(page, pageSize);
    }

    public async Task<List<ReservaResponseDto>> GetAllAsync(string? estado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idHabitacion = null, string? cliente = null, string? tipo = null)
    {
        await MarcarReservasVencidasAsync();

        var query = _db.Reservas
            .Include(r => r.IdClienteNavigation)
            .Include(r => r.IdHabitacionNavigation)
            .Include(r => r.IdEstadoReservaNavigation)
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
                r.IdEstadoReservaNavigation.Codigo ?? EstadoReservaCodigo.Code.Pendiente,
                r.IdClienteNavigation != null ? r.IdClienteNavigation.Documento : null,
                r.Observaciones,
                r.EsNoShow,
                r.IdReservaCorporativa
            ))
            .ToListAsync();
    }

    public async Task<List<LlegadaHoyDto>> GetLlegadasHoyAsync(string? estado = null)
    {
        await MarcarReservasVencidasAsync();

        var hoy = DateTime.UtcNow.Date;
        var manana = hoy.AddDays(1);

        var query = _db.Reservas
            .Include(r => r.IdClienteNavigation)
            .Include(r => r.IdHabitacionNavigation).ThenInclude(h => h!.IdTipoNavigation)
            .Include(r => r.IdReservaCorporativaNavigation).ThenInclude(rc => rc!.IdClienteEmpresaNavigation)
            .Include(r => r.IdEstadoReservaNavigation)
            .AsNoTracking()
            .Where(r => r.FechaEntradaPrevista >= hoy && r.FechaEntradaPrevista < manana);

        if (!string.IsNullOrEmpty(estado))
        {
            var estados = estado.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            query = query.Where(r => estados.Contains(r.IdEstadoReservaNavigation.Codigo));
        }

        return await query
            .OrderBy(r => r.FechaEntradaPrevista)
            .ThenBy(r => r.IdHabitacionNavigation != null ? r.IdHabitacionNavigation.NumeroHabitacion : "")
            .Select(r => new LlegadaHoyDto
            {
                IdReserva = r.IdReserva,
                ClienteNombre = r.IdClienteNavigation != null
                    ? $"{r.IdClienteNavigation.Nombres} {r.IdClienteNavigation.Apellidos}"
                    : null,
                DocumentoCliente = r.IdClienteNavigation != null ? r.IdClienteNavigation.Documento : null,
                IdHabitacion = r.IdHabitacionNavigation != null ? (int?)r.IdHabitacionNavigation.IdHabitacion : null,
                NumeroHabitacion = r.IdHabitacionNavigation != null ? r.IdHabitacionNavigation.NumeroHabitacion : null,
                TipoHabitacion = r.IdHabitacionNavigation != null && r.IdHabitacionNavigation.IdTipoNavigation != null
                    ? r.IdHabitacionNavigation.IdTipoNavigation.Nombre
                    : null,
                FechaEntradaPrevista = r.FechaEntradaPrevista,
                FechaSalidaPrevista = r.FechaSalidaPrevista,
                Observaciones = r.Observaciones,
                Estado = r.IdEstadoReservaNavigation.Codigo ?? EstadoReservaCodigo.Code.Pendiente,
                EmpresaCorporativa = r.IdReservaCorporativaNavigation != null && r.IdReservaCorporativaNavigation.IdClienteEmpresaNavigation != null
                    ? r.IdReservaCorporativaNavigation.IdClienteEmpresaNavigation.Nombres
                    : null,
                EsReservaCorporativa = r.IdReservaCorporativa != null,
                EsNoShow = r.EsNoShow,
            })
            .ToListAsync();
    }

    private async Task MarcarReservasVencidasAsync()
    {
        var hoy = DateTime.Today;
        try
        {
            var pendienteId = await _db.EstadosReserva
                .Where(e => e.Codigo == EstadoReservaCodigo.Code.Pendiente)
                .Select(e => e.IdEstadoReserva)
                .FirstOrDefaultAsync();
            var confirmadaId = await _db.EstadosReserva
                .Where(e => e.Codigo == EstadoReservaCodigo.Code.Confirmada)
                .Select(e => e.IdEstadoReserva)
                .FirstOrDefaultAsync();
            var vencidaId = await _db.EstadosReserva
                .Where(e => e.Codigo == EstadoReservaCodigo.Code.Vencida)
                .Select(e => e.IdEstadoReserva)
                .FirstOrDefaultAsync();

            if (vencidaId == 0) return;

            var afectadas = await _db.Reservas
                .Where(r => r.FechaSalidaPrevista < hoy
                    && (r.IdEstadoReserva == pendienteId
                        || r.IdEstadoReserva == confirmadaId))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.IdEstadoReserva, vencidaId));

            if (afectadas > 0)
                _logger.LogInformation("[Backend-Debug] Marcadas {Count} reservas como Vencidas (fecha salida < {Hoy})", afectadas, hoy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar reservas vencidas");
        }
    }

    public async Task<List<FechaOcupadaDto>> GetFechasOcupadasAsync(List<int>? idsHabitacion = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        await MarcarReservasVencidasAsync();

        var query = _db.Reservas
            .Include(r => r.IdEstadoReservaNavigation)
            .AsNoTracking()
            .Where(r => r.IdEstadoReservaNavigation.Codigo == EstadoReservaCodigo.Code.Pendiente
                     || r.IdEstadoReservaNavigation.Codigo == EstadoReservaCodigo.Code.Confirmada);

        if (idsHabitacion != null && idsHabitacion.Count > 0)
            query = query.Where(r => idsHabitacion.Contains(r.IdHabitacion));

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
