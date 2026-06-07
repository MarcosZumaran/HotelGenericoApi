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
}
