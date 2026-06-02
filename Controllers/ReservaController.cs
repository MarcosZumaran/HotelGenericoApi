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
    [ProducesResponseType(typeof(List<ReservaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReservaResponseDto>>> GetAll()
    {
        var reservas = await _reservaQueryService.GetAllAsync();
        return Ok(reservas);
    }
}
