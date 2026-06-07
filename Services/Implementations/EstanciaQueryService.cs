using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Constants;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Extensions;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class EstanciaQueryService : IEstanciaQueryService
{
    private readonly HotelDbContext _db;

    public EstanciaQueryService(HotelDbContext db)
    {
        _db = db;
    }

    public async Task<List<Estancia>> GetAllAsync()
    {
        return await _db.Estancias
            .Include(e => e.IdHabitacionNavigation).ThenInclude(h => h.IdTipoNavigation)
            .Include(e => e.IdClienteTitularNavigation)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<PagedResult<Estancia>> GetPagedAsync(int page, int pageSize)
    {
        var query = _db.Estancias
            .Include(e => e.IdHabitacionNavigation).ThenInclude(h => h.IdTipoNavigation)
            .Include(e => e.IdClienteTitularNavigation)
            .AsNoTracking();
        return await query.ToPagedResultAsync(page, pageSize);
    }

    public async Task<List<Estancia>> GetActivasAsync()
    {
        return await _db.Estancias
            .Include(e => e.IdHabitacionNavigation).ThenInclude(h => h.IdTipoNavigation)
            .Include(e => e.IdClienteTitularNavigation)
            .Where(e => e.IdEstadoEstancia == EstadoEstanciaCodigo.Activa)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Estancia?> GetByIdAsync(int id)
    {
        return await _db.Estancias
            .Include(e => e.IdHabitacionNavigation).ThenInclude(h => h.IdTipoNavigation)
            .Include(e => e.IdClienteTitularNavigation)
            .Include(e => e.ItemsEstancia!).ThenInclude(i => i.IdProductoNavigation)
            .Include(e => e.Huespedes!).ThenInclude(h => h.IdClienteNavigation)
            .FirstOrDefaultAsync(e => e.IdEstancia == id);
    }

    public async Task<List<ItemConsumoResponseDto>> GetConsumosAsync(int idEstancia)
    {
        return await _db.ItemsEstancia
            .Where(i => i.IdEstancia == idEstancia)
            .Include(i => i.IdProductoNavigation)
            .Select(i => new ItemConsumoResponseDto(
                i.IdItem,
                i.IdProducto,
                i.IdProductoNavigation!.Nombre,
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
            .Include(r => r.IdClienteNavigation)
            .Select(r => new ReservaResponseDto(
                r.IdReserva,
                r.IdHabitacion,
                r.IdHabitacionNavigation.NumeroHabitacion,
                r.IdClienteNavigation.Nombres + " " + r.IdClienteNavigation.Apellidos,
                r.FechaEntradaPrevista,
                r.FechaSalidaPrevista,
                r.MontoTotal,
                r.Estado ?? EstadoReservaCodigo.Code.Pendiente,
                r.IdClienteNavigation.Documento,
                r.Observaciones,
                r.EsNoShow
            ))
            .AsNoTracking()
            .ToListAsync();
    }
}
