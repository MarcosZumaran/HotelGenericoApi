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
    private readonly IPdfService _pdfService;
    private readonly ILogger<ReporteController> _logger;

    public ReporteController(IReporteService reporteService, IExcelExportService excelExportService, IPdfService pdfService, ILogger<ReporteController> logger)
    {
        _reporteService = reporteService;
        _excelExportService = excelExportService;
        _pdfService = pdfService;
        _logger = logger;
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

    /// <summary>Obtiene la previsión de ocupación para los próximos días.</summary>
    /// <param name="dias">Cantidad de días a proyectar (default: 7).</param>
    [HttpGet("prevision-ocupacion")]
    [ProducesResponseType(typeof(List<PrevisionOcupacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PrevisionOcupacionDto>>> GetPrevisionOcupacion([FromQuery] int dias = 7)
    {
        var result = await _reporteService.GetPrevisionOcupacionAsync(dias);
        return Ok(result);
    }

    /// <summary>Obtiene el tiempo medio de limpieza.</summary>
    [HttpGet("tiempo-medio-limpieza")]
    [ProducesResponseType(typeof(TiempoMedioLimpiezaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TiempoMedioLimpiezaDto>> GetTiempoMedioLimpieza()
    {
        var result = await _reporteService.GetTiempoMedioLimpiezaAsync();
        return Ok(result);
    }

    /// <summary>Obtiene la tasa de cancelaciones en los últimos meses.</summary>
    /// <param name="meses">Cantidad de meses hacia atrás (default: 3).</param>
    [HttpGet("tasa-cancelaciones")]
    [ProducesResponseType(typeof(TasaCancelacionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TasaCancelacionDto>> GetTasaCancelaciones([FromQuery] int meses = 3)
    {
        var result = await _reporteService.GetTasaCancelacionesAsync(meses);
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

    /// <summary>Obtiene el estado de par stock de todos los productos.</summary>
    [HttpGet("par-stock")]
    [ProducesResponseType(typeof(List<ParStockItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ParStockItemDto>>> GetParStock()
    {
        var result = await _reporteService.GetParStockAsync();
        return Ok(result);
    }

    /// <summary>Obtiene productos con stock crítico (Stock ≤ Stock Mínimo).</summary>
    [HttpGet("stock-critico")]
    [ProducesResponseType(typeof(List<StockCriticoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StockCriticoDto>>> GetStockCritico()
    {
        var result = await _reporteService.GetStockCriticoAsync();
        return Ok(result);
    }

    /// <summary>Exporta el par stock a PDF.</summary>
    [HttpGet("par-stock/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarParStockPdf()
    {
        var bytes = await _pdfService.GenerarPdfParStockAsync();
        return File(bytes, "application/pdf", "par_stock.pdf");
    }

    /// <summary>Exporta el par stock a Excel.</summary>
    [HttpGet("par-stock/excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarParStockExcel()
    {
        var data = await _reporteService.GetParStockAsync();
        var bytes = _excelExportService.GenerateParStockExcel(data);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "par_stock.xlsx");
    }

    /// <summary>Obtiene el gasto total en amenities (desglosado por producto).</summary>
    /// <param name="dias">Cantidad de días hacia atrás (default: 30).</param>
    [HttpGet("gasto-amenities")]
    [ProducesResponseType(typeof(GastoAmenitiesResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<GastoAmenitiesResponseDto>> GetGastoAmenities([FromQuery] int dias = 30)
    {
        var result = await _reporteService.GetGastoAmenitiesAsync(dias);
        return Ok(result);
    }

    /// <summary>Obtiene la evolución diaria del gasto en amenities.</summary>
    /// <param name="dias">Cantidad de días hacia atrás (default: 30).</param>
    [HttpGet("gasto-amenities-diario")]
    [ProducesResponseType(typeof(List<GastoAmenitiesDiarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GastoAmenitiesDiarioDto>>> GetGastoAmenitiesDiario([FromQuery] int dias = 30)
    {
        var result = await _reporteService.GetGastoAmenitiesDiarioAsync(dias);
        return Ok(result);
    }

    /// <summary>Exporta el reporte de gasto en amenities a Excel (2 hojas).</summary>
    /// <param name="dias">Cantidad de días hacia atrás (default: 30).</param>
    [HttpGet("gasto-amenities/excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportarGastoAmenitiesExcel([FromQuery] int dias = 30)
    {
        if (dias <= 0 || dias > 365)
            return BadRequest(new { mensaje = "El parámetro 'dias' debe estar entre 1 y 365." });

        try
        {
            var resumen = await _reporteService.GetGastoAmenitiesAsync(dias);
            var diario = await _reporteService.GetGastoAmenitiesDiarioAsync(dias);
            var bytes = _excelExportService.GenerateGastoAmenitiesExcel(resumen, diario);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"gasto_amenities_{dias}dias.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar Excel de gasto en amenities");
            return StatusCode(500, new { mensaje = "Error interno al generar el archivo Excel." });
        }
    }
}
