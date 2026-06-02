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
public class ReservaCorporativaController : ControllerBase
{
    private readonly IReservaCorporativaService _reservaCorporativaService;

    public ReservaCorporativaController(IReservaCorporativaService reservaCorporativaService)
    {
        _reservaCorporativaService = reservaCorporativaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var reservas = await _reservaCorporativaService.GetAllAsync();
        return Ok(reservas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var reserva = await _reservaCorporativaService.GetByIdAsync(id);
        if (reserva == null) return NotFound();
        return Ok(reserva);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservaCorporativaCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var reserva = await _reservaCorporativaService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = reserva.IdReservaCorporativa }, reserva);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReservaCorporativaCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _reservaCorporativaService.UpdateAsync(id, dto, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _reservaCorporativaService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/finalizar")]
    public async Task<IActionResult> Finalizar(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        try
        {
            var reserva = await _reservaCorporativaService.FinalizarAsync(id, userId);
            return Ok(reserva);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
