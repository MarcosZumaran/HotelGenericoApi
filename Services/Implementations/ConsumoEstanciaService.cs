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
