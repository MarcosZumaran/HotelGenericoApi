using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Data;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class ConsumoEstanciaService : IConsumoEstanciaService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<ConsumoEstanciaService> _logger;

    public ConsumoEstanciaService(HotelDbContext db, ILogger<ConsumoEstanciaService> logger)
    {
        _db = db;
        _logger = logger;
    }

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

    public async Task<bool> AddConsumoBatchAsync(int idEstancia, List<ItemEstancia> items, int idUsuario)
    {
        var estancia = await _db.Estancias
            .Include(e => e.IdHabitacionNavigation)
            .FirstOrDefaultAsync(e => e.IdEstancia == idEstancia);

        if (estancia == null) return false;

        var idHabitacion = estancia.IdHabitacion;

        foreach (var item in items)
        {
            var producto = await _db.Productos.FindAsync(item.IdProducto);
            if (producto == null) continue;

            if (producto.Stock < item.Cantidad)
                throw new InvalidOperationException($"Stock insuficiente para {producto.Nombre}");

            item.IdEstancia = idEstancia;
            item.FechaRegistro = DateTime.UtcNow;

            if (producto.EsAmenidad)
            {
                item.PrecioUnitario = 0;
                item.Subtotal = 0;
            }
            else
            {
                item.PrecioUnitario = producto.PrecioUnitario;
                item.Subtotal = item.Cantidad * item.PrecioUnitario;
            }

            producto.Stock -= item.Cantidad;
            _db.ItemsEstancia.Add(item);

            var stockHab = await _db.StockHabitaciones
                .FirstOrDefaultAsync(sh => sh.IdHabitacion == idHabitacion && sh.IdProducto == item.IdProducto);

            if (stockHab != null)
            {
                if (stockHab.CantidadActual < item.Cantidad)
                    throw new InvalidOperationException($"Stock de habitación insuficiente para {producto.Nombre} (disponible: {stockHab.CantidadActual}, solicitado: {item.Cantidad})");

                stockHab.CantidadActual -= item.Cantidad;
                stockHab.FechaActualizacion = DateTime.UtcNow;
            }

            var codigoMovimiento = producto.EsAmenidad ? "AMENIDAD" : "CONSUMO";
            var motivo = producto.EsAmenidad
                ? $"Amenidad #{idEstancia}: {producto.Nombre} (costo interno)"
                : $"Consumo en estancia #{idEstancia}";

            _db.MovimientosStock.Add(new MovimientoStock
            {
                IdProducto = item.IdProducto,
                IdHabitacion = idHabitacion,
                IdEstancia = idEstancia,
                CodigoTipoMovimiento = codigoMovimiento,
                IdUsuario = idUsuario,
                Cantidad = item.Cantidad,
                StockAnterior = stockHab?.CantidadActual + item.Cantidad,
                StockNuevo = stockHab?.CantidadActual,
                CostoUnitario = producto.PrecioUnitario,
                Motivo = motivo,
                FechaMovimiento = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Consumo batch añadido a estancia {IdEstancia}: {Count} items", idEstancia, items.Count);
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
}
