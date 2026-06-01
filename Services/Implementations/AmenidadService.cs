using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class AmenidadService : IAmenidadService
{
    private readonly HotelDbContext _db;

    public AmenidadService(HotelDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Inicializa el stock de amenidades para una habitación recién ocupada.
    /// Toma los productos marcados como EsAmenidad=true y con StockPorHabitacion > 0,
    /// y los inserta en stock_habitacion con la cantidad base.
    /// </summary>
    public async Task InicializarStockHabitacionAsync(int idHabitacion)
    {
        var amenidades = await _db.Productos
            .Where(p => p.EsAmenidad && p.StockPorHabitacion.HasValue && p.StockPorHabitacion > 0)
            .ToListAsync();

        foreach (var producto in amenidades)
        {
            var stockActual = await _db.StockHabitaciones
                .FirstOrDefaultAsync(s => s.IdHabitacion == idHabitacion && s.IdProducto == producto.IdProducto);

            if (stockActual == null)
            {
                _db.StockHabitaciones.Add(new StockHabitacion
                {
                    IdHabitacion = idHabitacion,
                    IdProducto = producto.IdProducto,
                    CantidadActual = producto.StockPorHabitacion!.Value
                });
            }
            else
            {
                stockActual.CantidadActual = producto.StockPorHabitacion!.Value;
            }
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Repone el stock de amenidades a su cantidad base (stock_por_habitacion)
    /// cuando la habitación se marca como disponible (limpieza completada).
    /// </summary>
    public async Task ReponerStockHabitacionAsync(int idHabitacion)
    {
        var amenidades = await _db.Productos
            .Where(p => p.EsAmenidad && p.StockPorHabitacion.HasValue && p.StockPorHabitacion > 0)
            .ToListAsync();

        foreach (var producto in amenidades)
        {
            var stockHabitacion = await _db.StockHabitaciones
                .FirstOrDefaultAsync(s => s.IdHabitacion == idHabitacion && s.IdProducto == producto.IdProducto);

            if (stockHabitacion == null)
            {
                _db.StockHabitaciones.Add(new StockHabitacion
                {
                    IdHabitacion = idHabitacion,
                    IdProducto = producto.IdProducto,
                    CantidadActual = producto.StockPorHabitacion!.Value
                });
            }
            else
            {
                stockHabitacion.CantidadActual = producto.StockPorHabitacion!.Value;
            }
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Consume una amenidad (reduce el stock de la habitación).
    /// Si EsCargableAlHuésped es true, además registra un consumo en item_estancia.
    /// </summary>
    public async Task<StockHabitacionDto?> ConsumirAmenidadAsync(int idHabitacion, ConsumirAmenidadDto dto)
    {
        var stock = await _db.StockHabitaciones
            .Include(s => s.Producto)
            .FirstOrDefaultAsync(s => s.IdHabitacion == idHabitacion && s.IdProducto == dto.ProductoId);

        if (stock == null)
            throw new InvalidOperationException("Este producto no está registrado como amenidad en esta habitación.");

        if (stock.CantidadActual < dto.Cantidad)
            throw new InvalidOperationException($"Stock insuficiente de {stock.Producto!.Nombre}. Disponible: {stock.CantidadActual}");

        stock.CantidadActual -= dto.Cantidad;
        await _db.SaveChangesAsync();

        // Si es cargable, necesitamos asociar este consumo a una estancia activa.
        // Para eso necesitamos saber qué estancia está ocupando la habitación actualmente.
        if (dto.EsCargableAlHuésped)
        {
            var estanciaActiva = await _db.Estancias
                .FirstOrDefaultAsync(e => e.IdHabitacion == idHabitacion && e.Estado == "Activa" && e.FechaCheckoutReal == null);

            if (estanciaActiva == null)
                throw new InvalidOperationException("No hay una estancia activa para esta habitación.");

            var item = new ItemEstancia
            {
                IdEstancia = estanciaActiva.IdEstancia,
                IdProducto = dto.ProductoId,
                Cantidad = dto.Cantidad,
                PrecioUnitario = stock.Producto!.PrecioUnitario,
                FechaRegistro = DateTime.UtcNow,
                Subtotal = dto.Cantidad * stock.Producto.PrecioUnitario
            };
            _db.ItemsEstancia.Add(item);
            await _db.SaveChangesAsync();
        }

        return new StockHabitacionDto
        {
            IdStock = stock.IdStock,
            IdHabitacion = stock.IdHabitacion,
            NumeroHabitacion = (await _db.Habitaciones.FindAsync(idHabitacion))?.NumeroHabitacion ?? "",
            IdProducto = stock.IdProducto,
            NombreProducto = stock.Producto!.Nombre,
            CantidadActual = stock.CantidadActual,
            StockBase = stock.Producto.StockPorHabitacion
        };
    }

    public async Task<List<StockHabitacionDto>> GetStockHabitacionAsync(int idHabitacion)
    {
        var stock = await _db.StockHabitaciones
            .Include(s => s.Producto)
            .Where(s => s.IdHabitacion == idHabitacion)
            .Select(s => new StockHabitacionDto
            {
                IdStock = s.IdStock,
                IdHabitacion = s.IdHabitacion,
                NumeroHabitacion = s.Habitacion != null ? s.Habitacion.NumeroHabitacion : "",
                IdProducto = s.IdProducto,
                NombreProducto = s.Producto != null ? s.Producto.Nombre : "",
                CantidadActual = s.CantidadActual,
                StockBase = s.Producto != null ? s.Producto.StockPorHabitacion : 0
            })
            .ToListAsync();

        return stock;
    }

    public async Task<bool> ReponerAmenidadIndividualAsync(int idHabitacion, int idProducto, int cantidad)
    {
        var stock = await _db.StockHabitaciones
            .FirstOrDefaultAsync(s => s.IdHabitacion == idHabitacion && s.IdProducto == idProducto);

        if (stock == null)
        {
            stock = new StockHabitacion
            {
                IdHabitacion = idHabitacion,
                IdProducto = idProducto,
                CantidadActual = cantidad
            };
            _db.StockHabitaciones.Add(stock);
        }
        else
        {
            stock.CantidadActual += cantidad;
        }

        await _db.SaveChangesAsync();
        return true;
    }
}
