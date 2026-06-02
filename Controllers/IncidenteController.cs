using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class IncidenteController : ControllerBase
{
    private readonly IIncidenteService _incidenteService;

    public IncidenteController(IIncidenteService incidenteService)
    {
        _incidenteService = incidenteService;
    }

    // INCIDENTES

    [HttpGet("incidentes")]
    public async Task<IActionResult> GetAllIncidentes()
    {
        var incidentes = await _incidenteService.GetAllIncidentesAsync();
        return Ok(incidentes);
    }

    [HttpGet("incidentes/{id}")]
    public async Task<IActionResult> GetIncidenteById(int id)
    {
        var incidente = await _incidenteService.GetIncidenteByIdAsync(id);
        if (incidente == null) return NotFound();
        return Ok(incidente);
    }

    [HttpGet("incidentes/habitacion/{idHabitacion}")]
    public async Task<IActionResult> GetIncidentesByHabitacion(int idHabitacion)
    {
        var incidentes = await _incidenteService.GetIncidentesByHabitacionAsync(idHabitacion);
        return Ok(incidentes);
    }

    [HttpPost("incidentes")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateIncidente([FromForm] IncidenteCreateDto dto, IFormFile? imagen)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        try
        {
            var incidente = await _incidenteService.CreateIncidenteAsync(dto, userId, imagen);
            return CreatedAtAction(nameof(GetIncidenteById), new { id = incidente.IdIncidente }, incidente);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("incidentes/{id}/resolver")]
    public async Task<IActionResult> ResolverIncidente(int id)
    {
        var result = await _incidenteService.ResolverIncidenteAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Incidente marcado como resuelto" });
    }

    [HttpPatch("incidentes/{id}/cobrar")]
    public async Task<IActionResult> MarcarCobrado(int id, [FromQuery] bool cobrado)
    {
        var result = await _incidenteService.MarcarCobradoAsync(id, cobrado);
        if (!result) return NotFound();
        return Ok(new { message = $"Incidente {(cobrado ? "cobrado" : "no cobrado")}" });
    }

    // OBJETOS PERDIDOS

    [HttpGet("objetos")]
    public async Task<IActionResult> GetAllObjetos()
    {
        var objetos = await _incidenteService.GetAllObjetosPerdidosAsync();
        return Ok(objetos);
    }

    [HttpGet("objetos/pendientes")]
    public async Task<IActionResult> GetObjetosPendientes()
    {
        var objetos = await _incidenteService.GetObjetosPendientesAsync();
        return Ok(objetos);
    }

    [HttpGet("objetos/{id}")]
    public async Task<IActionResult> GetObjetoById(int id)
    {
        var objeto = await _incidenteService.GetObjetoPerdidoByIdAsync(id);
        if (objeto == null) return NotFound();
        return Ok(objeto);
    }

    [HttpPost("objetos")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateObjeto([FromForm] ObjetoPerdidoCreateDto dto, IFormFile? imagen)
    {
        try
        {
            var objeto = await _incidenteService.CreateObjetoPerdidoAsync(dto, imagen);
            return CreatedAtAction(nameof(GetObjetoById), new { id = objeto.IdObjeto }, objeto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("objetos/{id}/entregar")]
    public async Task<IActionResult> EntregarObjeto(int id, [FromQuery] string entregadoA)
    {
        if (string.IsNullOrWhiteSpace(entregadoA))
            return BadRequest("Debe especificar a quién se entregó el objeto.");

        var result = await _incidenteService.EntregarObjetoAsync(id, entregadoA);
        if (!result) return NotFound();
        return Ok(new { message = $"Objeto entregado a {entregadoA}" });
    }

    [HttpPatch("objetos/{id}/desechar")]
    public async Task<IActionResult> DesecharObjeto(int id)
    {
        var result = await _incidenteService.DesecharObjetoAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Objeto marcado como desechado" });
    }
}
