using Microsoft.AspNetCore.Mvc;
using HotelGenericoApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PdfController : ControllerBase
{
    private readonly IPdfService _pdfService;
    public PdfController(IPdfService pdfService) => _pdfService = pdfService;

    [HttpGet("Comprobante/{id}")]
    public async Task<IActionResult> GetComprobantePdf(int id)
    {
        try
        {
            var bytes = await _pdfService.GenerarPdfComprobanteAsync(id);
            return File(bytes, "application/pdf", $"comprobante_{id}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpGet("Venta/{idVenta}")]
    public async Task<IActionResult> GetVentaPdf(int idVenta)
    {
        try
        {
            var bytes = await _pdfService.GenerarPdfVentaAsync(idVenta);
            return File(bytes, "application/pdf", $"venta_{idVenta}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpGet("Estancia/{idEstancia}")]
    public async Task<IActionResult> GetEstanciaPdf(int idEstancia)
    {
        try
        {
            var bytes = await _pdfService.GenerarPdfEstanciaAsync(idEstancia);
            return File(bytes, "application/pdf", $"estancia_{idEstancia}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpGet("CierreCaja")]
    public async Task<IActionResult> GetCierreCajaPdf([FromQuery] DateOnly fecha)
    {
        try
        {
            var bytes = await _pdfService.GenerarPdfCierreCajaAsync(fecha);
            return File(bytes, "application/pdf", $"cierre_caja_{fecha:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}