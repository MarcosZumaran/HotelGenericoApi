using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Constants;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class AmenidadService : IAmenidadService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<AmenidadService> _logger;

    public AmenidadService(HotelDbContext db, ILogger<AmenidadService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Inicializa el stock de amenidades para una habitación recién ocupada.
    /// Toma los productos marcados como EsAmenidad=true y con StockPorHabitacion > 0,
    /// y los inserta en stock_habitacion con la cantidad base.
    /// </summary>
    public async Task InicializarStockHabitacionAsync(int idHabitacion)
    {
        // Primero, obtener las amenidades personalizadas de la habitación
        var amenidadesPersonalizadas = await _db.HabitacionAmenidades
            .Include(ha => ha.IdProductoNavigation)
            .Where(ha => ha.IdHabitacion == idHabitacion)
            .ToListAsync();

        List<Producto> productosAInicializar;

        if (amenidadesPersonalizadas.Any())
        {
            // Usar las amenidades personalizadas
            productosAInicializar = amenidadesPersonalizadas.Select(ha => ha.IdProductoNavigation!).ToList();
        }
        else
        {
            // Usar las amenidades globales (todas con es_amenidad = true y stock_por_habitacion > 0)
            productosAInicializar = await _db.Productos
                .Where(p => p.EsAmenidad && p.StockPorHabitacion.HasValue && p.StockPorHabitacion > 0)
                .ToListAsync();
        }

        foreach (var producto in productosAInicializar)
        {
            var stockActual = await _db.StockHabitaciones
                .FirstOrDefaultAsync(s => s.IdHabitacion == idHabitacion && s.IdProducto == producto.IdProducto);

            var cantidadBase = amenidadesPersonalizadas
                .FirstOrDefault(ha => ha.IdProducto == producto.IdProducto)?.CantidadBase
                ?? producto.StockPorHabitacion ?? 0;

            if (stockActual == null)
            {
                _db.StockHabitaciones.Add(new StockHabitacion
                {
                    IdHabitacion = idHabitacion,
                    IdProducto = producto.IdProducto,
                    CantidadActual = cantidadBase
                });
            }
            else
            {
                stockActual.CantidadActual = cantidadBase;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<AmenidadEstadoDto>> GetAmenidadesEstadoAsync(int idHabitacion)
    {
        var amenidadesPersonalizadas = await _db.HabitacionAmenidades
            .Include(ha => ha.IdProductoNavigation)
            .Where(ha => ha.IdHabitacion == idHabitacion)
            .ToListAsync();

        List<(int IdProducto, string Nombre, int CantidadBase, bool EsAmenidad)> baseList;

        if (amenidadesPersonalizadas.Any())
        {
            baseList = amenidadesPersonalizadas
                .Select(ha => (ha.IdProducto, ha.Producto?.Nombre ?? "", ha.CantidadBase, true))
                .ToList();
        }
        else
        {
            var productos = await _db.Productos
                .Where(p => p.EsAmenidad && p.StockPorHabitacion.HasValue && p.StockPorHabitacion > 0)
                .ToListAsync();
            baseList = productos
                .Select(p => (p.IdProducto, p.Nombre, p.StockPorHabitacion!.Value, true))
                .ToList();
        }

        var stocks = await _db.StockHabitaciones
            .Where(s => s.IdHabitacion == idHabitacion)
            .ToListAsync();

        var stockDict = stocks.ToDictionary(s => s.IdProducto, s => s.CantidadActual);

        return baseList.Select(b => new AmenidadEstadoDto
        {
            IdProducto = b.IdProducto,
            Nombre = b.Nombre,
            CantidadBase = b.CantidadBase,
            CantidadActual = stockDict.GetValueOrDefault(b.IdProducto, 0),
            Diferencia = Math.Max(0, b.CantidadBase - stockDict.GetValueOrDefault(b.IdProducto, 0)),
            EsAmenidad = b.EsAmenidad,
        }).ToList();
    }

    public async Task<int> ReponerAmenidadesHabitacionAsync(int idHabitacion, int idUsuario)
    {
        var amenidades = await GetAmenidadesEstadoAsync(idHabitacion);
        var pendientes = amenidades.Where(a => a.Diferencia > 0).ToList();
        if (!pendientes.Any())
        {
            _logger.LogInformation("Reposicion amenidades Hab#{Hab}: sin pendientes", idHabitacion);
            return 0;
        }

        _logger.LogInformation("Reposicion amenidades Hab#{Hab}: {Count} productos por reponer", idHabitacion, pendientes.Count);

        var codigoRepo = await AsegurarTipoMovimientoReposicionAsync();

        var productosDict = await _db.Productos
            .Where(p => pendientes.Select(a => a.IdProducto).Contains(p.IdProducto))
            .ToDictionaryAsync(p => p.IdProducto);

        foreach (var am in pendientes)
        {
            var stockHab = await _db.StockHabitaciones
                .FirstOrDefaultAsync(s => s.IdHabitacion == idHabitacion && s.IdProducto == am.IdProducto);

            if (stockHab == null)
            {
                _db.StockHabitaciones.Add(new StockHabitacion
                {
                    IdHabitacion = idHabitacion,
                    IdProducto = am.IdProducto,
                    CantidadActual = am.Diferencia,
                    FechaActualizacion = DateTime.UtcNow,
                });
            }
            else
            {
                stockHab.CantidadActual += am.Diferencia;
                stockHab.FechaActualizacion = DateTime.UtcNow;
            }

            if (productosDict.TryGetValue(am.IdProducto, out var producto))
            {
                var stockAnterior = producto.Stock;
                var descuento = Math.Min(am.Diferencia, producto.Stock);
                producto.Stock = Math.Max(0, producto.Stock - am.Diferencia);

                _db.MovimientosStock.Add(new MovimientoStock
                {
                    IdProducto = am.IdProducto,
                    IdHabitacion = idHabitacion,
                    CodigoTipoMovimiento = "REPOSICION",
                    IdUsuario = idUsuario,
                    Cantidad = am.Diferencia,
                    StockAnterior = stockAnterior,
                    StockNuevo = producto.Stock,
                    CostoUnitario = producto.PrecioUnitario,
                    Motivo = $"Reposicion de amenidad en habitacion #{idHabitacion}",
                    FechaMovimiento = DateTime.UtcNow,
                });

                if (descuento < am.Diferencia)
                {
                    _logger.LogWarning(
                        "Stock insuficiente para {Producto} (id={Id}): necesario={Necesario}, disponible={Disponible}. Se descuentan {Descuento}.",
                        producto.Nombre, producto.IdProducto, am.Diferencia, stockAnterior, descuento);
                }
                else
                {
                    _logger.LogDebug(
                        "Amenidad {Producto} (id={Id}): -{Cantidad} del stock general (anterior={Anterior}, nuevo={Nuevo})",
                        producto.Nombre, producto.IdProducto, am.Diferencia, stockAnterior, producto.Stock);
                }
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Reposicion amenidades Hab#{Hab}: {Count} productos repuestos", idHabitacion, pendientes.Count);
        return pendientes.Count;
    }

    private async Task<string> AsegurarTipoMovimientoReposicionAsync()
    {
        var existente = await _db.TiposMovimientoStock
            .FirstOrDefaultAsync(t => t.Codigo == "REPOSICION");
        if (existente != null) return "REPOSICION";

        _db.TiposMovimientoStock.Add(new TipoMovimientoStock
        {
            Codigo = "REPOSICION",
            Descripcion = "Reposicion de amenidad en habitacion",
        });
        await _db.SaveChangesAsync();
        return "REPOSICION";
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
            .Include(s => s.IdProductoNavigation)
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
                .FirstOrDefaultAsync(e => e.IdHabitacion == idHabitacion && e.IdEstadoEstanciaNavigation.Codigo == EstadoEstanciaCodigo.Code.Activa && e.FechaCheckoutReal == null);

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
            .Include(s => s.IdProductoNavigation)
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
