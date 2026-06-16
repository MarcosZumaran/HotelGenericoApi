using Microsoft.AspNetCore.Mvc;
using HotelGenericoApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ComprobanteController : ControllerBase
{
    private readonly IComprobanteService _service;
    private readonly IXmlComprobanteService _xmlService;

    public ComprobanteController(IComprobanteService service, IXmlComprobanteService xmlService)
    {
        _service = service;
        _xmlService = xmlService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10) => Ok(await _service.GetPagedAsync(page, pageSize));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("{id}/xml")]
    [Produces("application/xml")]
    public async Task<IActionResult> GetXml(int id)
    {
        var xml = await _xmlService.GenerarXmlComprobanteAsync(id);
        return Content(xml, "application/xml");
    }

    [HttpPost("{id}/enviar")]
    public async Task<IActionResult> MarcarEnviado(int id, [FromBody] string hashXml)
    {
        var updated = await _service.MarcarComoEnviadoAsync(id, hashXml);
        return updated ? NoContent() : NotFound();
    }
}
