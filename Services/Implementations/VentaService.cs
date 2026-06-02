using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class VentaService : IVentaService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<VentaService> _logger;

    public VentaService(HotelDbContext db, ILogger<VentaService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<VentaResponseDto>> GetAllAsync()
    {
        return await _db.Ventas
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.MetodoPagoNavigation)
            .Include(v => v.ItemVenta).ThenInclude(i => i.IdProductoNavigation)
            .AsNoTracking()
            .OrderByDescending(v => v.FechaVenta)
            .Select(v => new VentaResponseDto
            {
                IdVenta = v.IdVenta,
                IdCliente = v.IdCliente,
                ClienteNombre = v.IdClienteNavigation != null
                    ? $"{v.IdClienteNavigation.Nombres} {v.IdClienteNavigation.Apellidos}"
                    : null,
                FechaVenta = v.FechaVenta,
                Total = v.Total,
                MetodoPago = v.MetodoPagoNavigation != null
                    ? v.MetodoPagoNavigation.Descripcion
                    : v.MetodoPago,
                Items = v.ItemVenta.Select(i => new ItemVentaResponseDto
                {
                    IdItem = i.IdItem,
                    IdProducto = i.IdProducto,
                    NombreProducto = i.IdProductoNavigation != null ? i.IdProductoNavigation.Nombre : null,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Subtotal = i.Subtotal.GetValueOrDefault()
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<VentaResponseDto?> GetByIdAsync(int id)
    {
        var venta = await _db.Ventas
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.MetodoPagoNavigation)
            .Include(v => v.ItemVenta).ThenInclude(i => i.IdProductoNavigation)
            .FirstOrDefaultAsync(v => v.IdVenta == id);

        if (venta == null) return null;

        return new VentaResponseDto
        {
            IdVenta = venta.IdVenta,
            IdCliente = venta.IdCliente,
            ClienteNombre = venta.IdClienteNavigation != null
                ? $"{venta.IdClienteNavigation.Nombres} {venta.IdClienteNavigation.Apellidos}"
                : null,
            FechaVenta = venta.FechaVenta,
            Total = venta.Total,
            MetodoPago = venta.MetodoPagoNavigation != null
                ? venta.MetodoPagoNavigation.Descripcion
                : venta.MetodoPago,
            Items = venta.ItemVenta.Select(i => new ItemVentaResponseDto
            {
                IdItem = i.IdItem,
                IdProducto = i.IdProducto,
                NombreProducto = i.IdProductoNavigation != null ? i.IdProductoNavigation.Nombre : null,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                Subtotal = i.Subtotal.GetValueOrDefault()
            }).ToList()
        };
    }

    public async Task<VentaResponseDto> CreateAsync(VentaCreateDto dto, int idUsuario)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Calcular total y validar stock
            decimal total = 0;
            var items = new List<ItemVentum>();

            foreach (var itemDto in dto.Items)
            {
                var producto = await _db.Productos.FindAsync(itemDto.IdProducto)
                    ?? throw new InvalidOperationException($"Producto con ID {itemDto.IdProducto} no encontrado.");

                if (producto.Stock < itemDto.Cantidad)
                    throw new InvalidOperationException($"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}, solicitado: {itemDto.Cantidad}.");

                // Descontar stock
                producto.Stock -= itemDto.Cantidad;

                var item = new ItemVentum
                {
                    IdProducto = itemDto.IdProducto,
                    Cantidad = itemDto.Cantidad,
                    PrecioUnitario = producto.PrecioUnitario,
                };
                items.Add(item);
                total += producto.PrecioUnitario * itemDto.Cantidad;
            }

            // Cargar el cliente desde la BD si se especificó uno
            Cliente? cliente = null;
            if (dto.IdCliente.HasValue)
            {
                cliente = await _db.Clientes.FindAsync(dto.IdCliente.Value);
            }

            var venta = new Ventum
            {
                IdCliente = dto.IdCliente,
                IdUsuario = idUsuario,
                FechaVenta = DateTime.UtcNow,
                Total = total,
                MetodoPago = dto.MetodoPago,
                ItemVenta = items
            };

            _db.Ventas.Add(venta);
            await _db.SaveChangesAsync();

            // Generar comprobante automático (ahora con el cliente cargado)
            var comprobante = new Comprobante
            {
                IdVenta = venta.IdVenta,
                TipoComprobante = "03",
                Serie = "B001",
                Correlativo = await ObtenerSiguienteCorrelativoAsync(),
                FechaEmision = DateTime.UtcNow,
                MontoTotal = total,
                IgvMonto = total * 0.18m,
                ClienteDocumentoTipo = cliente?.TipoDocumento,
                ClienteDocumentoNum = cliente?.Documento,
                ClienteNombre = cliente != null
                    ? $"{cliente.Nombres} {cliente.Apellidos}"
                    : "CLIENTE ANONIMO",
                MetodoPago = dto.MetodoPago,
                IdEstadoSunat = 1
            };
            _db.Comprobantes.Add(comprobante);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Venta {Id} creada por usuario {Usuario}, total {Total}, comprobante {Serie}-{Correlativo}",
                venta.IdVenta, idUsuario, total, comprobante.Serie, comprobante.Correlativo);

            return await GetByIdAsync(venta.IdVenta)
                ?? throw new InvalidOperationException("Error al recuperar la venta creada.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var venta = await _db.Ventas
            .Include(v => v.ItemVenta)
            .FirstOrDefaultAsync(v => v.IdVenta == id);

        if (venta == null) return false;

        if (venta.ItemVenta != null)
        {
            foreach (var item in venta.ItemVenta)
            {
                var producto = await _db.Productos.FindAsync(item.IdProducto);
                if (producto != null)
                {
                    producto.Stock += item.Cantidad;
                }
            }
        }

        var comprobante = await _db.Comprobantes
            .FirstOrDefaultAsync(c => c.IdVenta == id);
        if (comprobante != null)
        {
            _db.Comprobantes.Remove(comprobante);
        }

        _db.Ventas.Remove(venta);
        await _db.SaveChangesAsync();

        _logger.LogWarning("Venta {Id} eliminada. Stock devuelto.", id);
        return true;
    }

    private async Task<int> ObtenerSiguienteCorrelativoAsync()
    {
        int ultimo = await _db.Comprobantes
            .Where(c => c.Serie == "B001")
            .MaxAsync(c => (int?)c.Correlativo) ?? 0;
        return ultimo + 1;
    }
}
