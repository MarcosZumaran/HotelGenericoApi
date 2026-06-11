using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class StockHabitacionService : IStockHabitacionService
{
    private readonly HotelDbContext _db;

    public StockHabitacionService(HotelDbContext db)
    {
        _db = db;
    }

    public async Task<List<StockHabitacionDto>> GetByHabitacionAsync(int idHabitacion)
    {
        return await _db.StockHabitaciones
            .AsNoTracking()
            .Where(s => s.IdHabitacion == idHabitacion)
            .Include(s => s.IdProductoNavigation)
                .ThenInclude(p => p.IdCategoriaNavigation)
            .Select(s => new StockHabitacionDto
            {
                IdStock = s.IdStock,
                IdHabitacion = s.IdHabitacion,
                NumeroHabitacion = s.IdHabitacionNavigation.NumeroHabitacion,
                IdProducto = s.IdProducto,
                NombreProducto = s.IdProductoNavigation.Nombre,
                CantidadActual = s.CantidadActual,
                StockBase = 0,
                EsAmenidad = s.IdProductoNavigation.EsAmenidad,
                PrecioUnitario = s.IdProductoNavigation.PrecioUnitario,
                Categoria = s.IdProductoNavigation.IdCategoriaNavigation != null
                    ? s.IdProductoNavigation.IdCategoriaNavigation.Nombre
                    : null
            })
            .ToListAsync();
    }

    public async Task<bool> ConsumirProductoAsync(int idStock, int cantidad)
    {
        var stock = await _db.StockHabitaciones.FindAsync(idStock);
        if (stock == null || stock.CantidadActual < cantidad) return false;

        stock.CantidadActual -= cantidad;
        stock.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
