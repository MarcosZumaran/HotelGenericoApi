using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HotelGenericoApi.Constants;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class ReporteService : IReporteService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<ReporteService> _logger;

    public ReporteService(HotelDbContext db, ILogger<ReporteService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<VCierreCajaDiario>> GetCierreCajaAsync(DateOnly fecha)
    {
        return await _db.VCierreCajaDiario
            .Where(c => c.Fecha == fecha)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<VEstadoHabitacion>> GetEstadoHabitacionesAsync()
    {
        return await _db.VEstadoHabitacion
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<VOcupacionDiaria>> GetOcupacionDiariaAsync(DateOnly fecha)
    {
        return await _db.VOcupacionDiaria
            .Where(o => o.Fecha == fecha)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<TopProductoDto>> GetTopProductosAsync(int dias)
    {
        var fechaLimite = DateTime.UtcNow.AddDays(-dias);
        _logger.LogInformation("[Backend-Debug] GetTopProductosAsync — dias={Dias}, fechaLimite={FechaLimite:yyyy-MM-dd}", dias, fechaLimite);

        try
        {
            var ventasQuery = await _db.ItemsVenta
                .Where(iv => iv.IdVentaNavigation.FechaVenta >= fechaLimite
                    && iv.IdProductoNavigation.EsVendibleEnTienda)
                .GroupBy(iv => iv.IdProductoNavigation.Nombre)
                .Select(g => new TopProductoDto
                {
                    Nombre = g.Key,
                    CantidadTotal = g.Sum(iv => iv.Cantidad),
                    IngresoTotal = g.Sum(iv => iv.Subtotal) ?? 0
                })
                .AsNoTracking()
                .ToListAsync();

            var consumosQuery = await _db.ItemsEstancia
                .Where(ie => ie.FechaRegistro >= fechaLimite
                    && ie.IdProductoNavigation.EsVendibleEnTienda)
                .GroupBy(ie => ie.IdProductoNavigation.Nombre)
                .Select(g => new TopProductoDto
                {
                    Nombre = g.Key,
                    CantidadTotal = g.Sum(ie => ie.Cantidad),
                    IngresoTotal = g.Sum(ie => ie.Subtotal) ?? 0
                })
                .AsNoTracking()
                .ToListAsync();

            return ventasQuery.Concat(consumosQuery)
                .GroupBy(tp => tp.Nombre)
                .Select(g => new TopProductoDto
                {
                    Nombre = g.Key,
                    CantidadTotal = g.Sum(tp => tp.CantidadTotal),
                    IngresoTotal = g.Sum(tp => tp.IngresoTotal)
                })
                .OrderByDescending(tp => tp.IngresoTotal)
                .Take(10)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Backend-Debug] Error en GetTopProductosAsync({Dias}): {Mensaje}", dias, ex.Message);
            throw;
        }
    }

    public async Task<List<PrevisionOcupacionDto>> GetPrevisionOcupacionAsync(int dias)
    {
        var hoy = DateTime.UtcNow.Date;
        var totalHabitaciones = await _db.Habitaciones.CountAsync();
        var fechaLimite = hoy.AddDays(dias);

        var resultado = new List<PrevisionOcupacionDto>();

        for (var dia = hoy; dia < fechaLimite; dia = dia.AddDays(1))
        {
            var dateOnly = DateOnly.FromDateTime(dia);

            var ocupadas = await _db.Estancias
                .Where(e => e.IdEstadoEstanciaNavigation.Codigo == "Activa"
                    && e.FechaCheckin <= dia
                    && (e.FechaCheckoutReal == null || e.FechaCheckoutReal >= dia))
                .CountAsync();

            var reservadas = await _db.Reservas
                .Where(r => r.IdEstadoReservaNavigation.Codigo == "Confirmada"
                    && r.FechaEntradaPrevista <= dia
                    && r.FechaSalidaPrevista >= dia)
                .CountAsync();

            resultado.Add(new PrevisionOcupacionDto
            {
                Fecha = dateOnly,
                Ocupadas = ocupadas + reservadas,
                TotalHabitaciones = totalHabitaciones,
                Porcentaje = totalHabitaciones > 0
                    ? Math.Round((decimal)(ocupadas + reservadas) / totalHabitaciones * 100, 1)
                    : 0
            });
        }

        return resultado;
    }

    public async Task<TiempoMedioLimpiezaDto> GetTiempoMedioLimpiezaAsync()
    {
        var limpiezaId = EstadoHabitacionCodigo.Limpieza;
        var disponibleId = EstadoHabitacionCodigo.Disponible;

        var limpiezasQuery = await _db.HistorialEstadoHabitaciones
            .Where(h => h.IdEstadoNuevo == limpiezaId)
            .OrderByDescending(h => h.FechaCambio)
            .Take(100)
            .AsNoTracking()
            .ToListAsync();

        var tiempos = new List<double>();

        foreach (var item in limpiezasQuery)
        {
            var finLimpieza = await _db.HistorialEstadoHabitaciones
                .Where(h => h.IdHabitacion == item.IdHabitacion
                    && h.IdEstadoNuevo == disponibleId
                    && h.FechaCambio > item.FechaCambio)
                .OrderBy(h => h.FechaCambio)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (finLimpieza != null)
            {
                var diffMinutes = (finLimpieza.FechaCambio - item.FechaCambio).TotalMinutes;
                if (diffMinutes > 0 && diffMinutes < 720)
                {
                    tiempos.Add(diffMinutes);
                }
            }
        }

        return new TiempoMedioLimpiezaDto
        {
            MinutosPromedio = tiempos.Count > 0 ? Math.Round(tiempos.Average(), 1) : 0,
            TotalLimpiezas = tiempos.Count
        };
    }

    public async Task<TasaCancelacionDto> GetTasaCancelacionesAsync(int meses)
    {
        var fechaLimite = DateTime.UtcNow.AddMonths(-meses);

        var total = await _db.Reservas
            .Where(r => r.FechaRegistro >= fechaLimite)
            .CountAsync();

        var canceladas = await _db.Reservas
            .Where(r => r.FechaRegistro >= fechaLimite
                && r.IdEstadoReserva == EstadoReservaCodigo.Cancelada)
            .CountAsync();

        return new TasaCancelacionDto
        {
            TotalReservas = total,
            Canceladas = canceladas,
            Tasa = total > 0 ? Math.Round((decimal)canceladas / total * 100, 1) : 0
        };
    }

    public async Task<List<ParStockItemDto>> GetParStockAsync()
    {
        var productos = await _db.Productos
            .Include(p => p.IdCategoriaNavigation)
            .Where(p => p.EsVendibleEnTienda)
            .AsNoTracking()
            .ToListAsync();

        return productos
            .Select(p => new ParStockItemDto(
                p.IdProducto,
                p.Nombre,
                p.IdCategoriaNavigation?.Nombre,
                p.Stock,
                p.StockMinimo,
                p.StockMinimo > 0
                    ? Math.Round((decimal)p.Stock / p.StockMinimo * 100m, 1)
                    : 100m,
                p.EsAmenidad,
                p.UnidadMedida
            ))
            .OrderBy(p => p.NivelPorcentaje)
            .ToList();
    }

    public async Task<List<StockCriticoDto>> GetStockCriticoAsync()
    {
        return await _db.Productos
            .Where(p => p.Stock <= p.StockMinimo && p.EsVendibleEnTienda)
            .AsNoTracking()
            .Select(p => new StockCriticoDto(
                p.IdProducto,
                p.Nombre,
                p.Stock,
                p.StockMinimo
            ))
            .ToListAsync();
    }

    public async Task<GastoAmenitiesResponseDto> GetGastoAmenitiesAsync(int dias)
    {
        var fechaLimite = DateTime.UtcNow.AddDays(-dias);

        var movimientos = await _db.MovimientosStock
            .Where(m => m.FechaMovimiento >= fechaLimite
                && (m.CodigoTipoMovimiento == "AMENIDAD" || m.CodigoTipoMovimiento == "REPOSICION")
                && m.CostoUnitario.HasValue)
            .Select(m => new { m.IdProducto, m.IdProductoNavigation.Nombre, m.Cantidad, m.CostoUnitario })
            .AsNoTracking()
            .ToListAsync();

        var detalle = movimientos
            .GroupBy(m => new { m.IdProducto, m.Nombre })
            .Select(g => new GastoAmenitiesDetalleDto(
                g.Key.IdProducto,
                g.Key.Nombre,
                g.Sum(m => m.Cantidad),
                g.Sum(m => m.Cantidad * m.CostoUnitario!.Value) / (g.Sum(m => m.Cantidad) == 0 ? 1 : g.Sum(m => m.Cantidad)),
                g.Sum(m => m.Cantidad * m.CostoUnitario!.Value)
            ))
            .OrderByDescending(d => d.CostoTotal)
            .ToList();

        return new GastoAmenitiesResponseDto(
            detalle.Sum(d => d.CostoTotal),
            dias,
            detalle
        );
    }

    public async Task<List<GastoAmenitiesDiarioDto>> GetGastoAmenitiesDiarioAsync(int dias)
    {
        var fechaLimite = DateTime.UtcNow.AddDays(-dias).Date;
        var hoy = DateTime.UtcNow.Date;

        var rawData = await _db.MovimientosStock
            .Where(m => m.FechaMovimiento >= fechaLimite
                && (m.CodigoTipoMovimiento == "AMENIDAD" || m.CodigoTipoMovimiento == "REPOSICION")
                && m.CostoUnitario.HasValue)
            .Select(m => new { Fecha = m.FechaMovimiento.Date, Costo = m.Cantidad * m.CostoUnitario!.Value })
            .AsNoTracking()
            .ToListAsync();

        var dailyData = rawData
            .GroupBy(m => m.Fecha)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Costo));

        var resultado = new List<GastoAmenitiesDiarioDto>();
        for (var fecha = fechaLimite; fecha <= hoy; fecha = fecha.AddDays(1))
        {
            resultado.Add(new GastoAmenitiesDiarioDto(
                DateOnly.FromDateTime(fecha),
                dailyData.GetValueOrDefault(fecha, 0m)
            ));
        }

        return resultado;
    }
}
