using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class StockHabitacionController : ControllerBase
{
    private readonly IStockHabitacionService _service;

    public StockHabitacionController(IStockHabitacionService service)
    {
        _service = service;
    }

    [HttpGet("habitacion/{idHabitacion}")]
    [ProducesResponseType(typeof(List<StockHabitacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StockHabitacionDto>>> GetByHabitacion(int idHabitacion)
    {
        var result = await _service.GetByHabitacionAsync(idHabitacion);
        return Ok(result);
    }
}
