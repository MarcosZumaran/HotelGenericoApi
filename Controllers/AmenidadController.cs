using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AmenidadController : ControllerBase
{
    private readonly IAmenidadService _amenidadService;

    public AmenidadController(IAmenidadService amenidadService)
    {
        _amenidadService = amenidadService;
    }

    /// <summary>
    /// Obtiene el stock actual de amenidades en una habitación.
    /// </summary>
    [HttpGet("habitacion/{idHabitacion}")]
    public async Task<IActionResult> GetStockHabitacion(int idHabitacion)
    {
        var stock = await _amenidadService.GetStockHabitacionAsync(idHabitacion);
        return Ok(stock);
    }

    /// <summary>
    /// Consume una amenidad (reduce stock y opcionalmente lo cobra al huésped).
    /// </summary>
    [HttpPost("habitacion/{idHabitacion}/consumir")]
    public async Task<IActionResult> ConsumirAmenidad(int idHabitacion, [FromBody] ConsumirAmenidadDto dto)
    {
        try
        {
            var result = await _amenidadService.ConsumirAmenidadAsync(idHabitacion, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Repone una amenidad específica en una habitación (usado por limpieza o reposición manual).
    /// </summary>
    [HttpPost("habitacion/{idHabitacion}/reponer")]
    public async Task<IActionResult> ReponerAmenidad(int idHabitacion, [FromBody] ReponerAmenidadDto dto)
    {
        var result = await _amenidadService.ReponerAmenidadIndividualAsync(idHabitacion, dto.ProductoId, dto.Cantidad);
        if (!result) return BadRequest();
        return Ok(new { message = "Stock repuesto correctamente" });
    }

    /// <summary>
    /// Repone todas las amenidades de una habitación a su cantidad base (ejecutado tras limpieza).
    /// </summary>
    [HttpPost("habitacion/{idHabitacion}/reponer-todo")]
    public async Task<IActionResult> ReponerTodo(int idHabitacion)
    {
        await _amenidadService.ReponerStockHabitacionAsync(idHabitacion);
        return Ok(new { message = "Todas las amenidades repuestas a su cantidad base" });
    }
}

// DTO auxiliar para reposición manual
public class ReponerAmenidadDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}
