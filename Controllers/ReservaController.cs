using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[EnableRateLimiting("authenticated")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ReservaController : ControllerBase
{
    private readonly IReservaQueryService _reservaQueryService;
    private readonly IReservaCommandService _reservaCommandService;

    public ReservaController(IReservaQueryService reservaQueryService, IReservaCommandService reservaCommandService)
    {
        _reservaQueryService = reservaQueryService;
        _reservaCommandService = reservaCommandService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReservaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReservaResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var reservas = await _reservaQueryService.GetPagedAsync(page, pageSize);
        return Ok(reservas);
    }

    [HttpGet("todas")]
    [ProducesResponseType(typeof(List<ReservaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReservaResponseDto>>> GetAllFiltered(
        [FromQuery] string? estado,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int? idHabitacion,
        [FromQuery] string? cliente,
        [FromQuery] string? tipo)
        {
            var reservas = await _reservaQueryService.GetAllAsync(estado, fechaDesde, fechaHasta, idHabitacion, cliente, tipo);
        return Ok(reservas);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservaResponseDto>> GetById(int id)
    {
        var reservas = await _reservaQueryService.GetAllAsync();
        var reserva = reservas.FirstOrDefault(r => r.IdReserva == id);
        if (reserva == null) return NotFound();
        return Ok(reserva);
    }

    [HttpGet("llegadas-hoy")]
    [ProducesResponseType(typeof(List<LlegadaHoyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LlegadaHoyDto>>> GetLlegadasHoy(
        [FromQuery] string? estado = "Pendiente,Confirmada")
    {
        var llegadas = await _reservaQueryService.GetLlegadasHoyAsync(estado);
        return Ok(llegadas);
    }

    [HttpGet("fechas-ocupadas")]
    [ProducesResponseType(typeof(List<FechaOcupadaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FechaOcupadaDto>>> GetFechasOcupadas(
        [FromQuery] string? idHabitacion,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        List<int>? ids = null;
        if (!string.IsNullOrWhiteSpace(idHabitacion))
        {
            ids = idHabitacion
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse)
                .ToList();
        }

        var fechas = await _reservaQueryService.GetFechasOcupadasAsync(ids, fechaDesde, fechaHasta);
        return Ok(fechas);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _reservaCommandService.CancelarReservaAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
