using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
                .Where(iv => iv.IdVentaNavigation.FechaVenta >= fechaLimite)
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
                .Where(ie => ie.FechaRegistro >= fechaLimite)
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
}
