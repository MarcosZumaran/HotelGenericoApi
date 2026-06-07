using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[EnableRateLimiting("authenticated")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ReporteController : ControllerBase
{
    private readonly IReporteService _reporteService;
    private readonly IExcelExportService _excelExportService;

    public ReporteController(IReporteService reporteService, IExcelExportService excelExportService)
    {
        _reporteService = reporteService;
        _excelExportService = excelExportService;
    }

    /// <summary>Obtiene el cierre de caja diario con detalle de ingresos y egresos.</summary>
    /// <param name="fecha">Fecha del cierre (yyyy-MM-dd).</param>
    [HttpGet("cierre-caja")]
    [ProducesResponseType(typeof(List<VCierreCajaDiario>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VCierreCajaDiario>>> GetCierreCaja([FromQuery] DateOnly fecha)
    {
        var result = await _reporteService.GetCierreCajaAsync(fecha);
        return Ok(result);
    }

    /// <summary>Obtiene el estado actual de todas las habitaciones.</summary>
    [HttpGet("estado-habitaciones")]
    [ProducesResponseType(typeof(List<VEstadoHabitacion>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VEstadoHabitacion>>> GetEstadoHabitaciones()
    {
        var result = await _reporteService.GetEstadoHabitacionesAsync();
        return Ok(result);
    }

    /// <summary>Obtiene el reporte de ocupación diaria.</summary>
    /// <param name="fecha">Fecha del reporte (yyyy-MM-dd).</param>
    [HttpGet("ocupacion-diaria")]
    [ProducesResponseType(typeof(List<VOcupacionDiaria>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VOcupacionDiaria>>> GetOcupacionDiaria([FromQuery] DateOnly fecha)
    {
        var result = await _reporteService.GetOcupacionDiariaAsync(fecha);
        return Ok(result);
    }

    /// <summary>Obtiene el top de productos más vendidos.</summary>
    /// <param name="dias">Cantidad de días hacia atrás para el reporte.</param>
    [HttpGet("top-productos")]
    [ProducesResponseType(typeof(List<TopProductoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TopProductoDto>>> GetTopProductos([FromQuery] int dias = 30)
    {
        var result = await _reporteService.GetTopProductosAsync(dias);
        return Ok(result);
    }

    /// <summary>Exporta el cierre de caja a Excel.</summary>
    [HttpGet("cierre-caja/excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarCierreCajaExcel([FromQuery] DateOnly fecha)
    {
        var data = await _reporteService.GetCierreCajaAsync(fecha);
        var bytes = _excelExportService.GenerateCierreCajaExcel(data, fecha);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"cierre_caja_{fecha:yyyy-MM-dd}.xlsx");
    }

    /// <summary>Exporta la ocupación diaria a Excel.</summary>
    [HttpGet("ocupacion-diaria/excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarOcupacionDiariaExcel([FromQuery] DateOnly fecha)
    {
        var data = await _reporteService.GetOcupacionDiariaAsync(fecha);
        var bytes = _excelExportService.GenerateOcupacionDiariaExcel(data, fecha);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ocupacion_diaria_{fecha:yyyy-MM-dd}.xlsx");
    }
}
