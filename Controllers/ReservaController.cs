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

    public ReservaController(IReservaQueryService reservaQueryService)
    {
        _reservaQueryService = reservaQueryService;
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

    [HttpGet("fechas-ocupadas")]
    [ProducesResponseType(typeof(List<FechaOcupadaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FechaOcupadaDto>>> GetFechasOcupadas(
        [FromQuery] int? idHabitacion,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        var fechas = await _reservaQueryService.GetFechasOcupadasAsync(idHabitacion, fechaDesde, fechaHasta);
        return Ok(fechas);
    }
}
