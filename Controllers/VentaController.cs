using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[EnableRateLimiting("authenticated")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class VentaController : ControllerBase
{
    private readonly IVentaService _ventaService;

    public VentaController(IVentaService ventaService)
    {
        _ventaService = ventaService;
    }

    /// <summary>Obtiene todas las ventas registradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VentaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<VentaResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var ventas = await _ventaService.GetPagedAsync(page, pageSize);
        return Ok(ventas);
    }

    /// <summary>Obtiene una venta por su ID con ítems y producto.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VentaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VentaResponseDto>> GetById(int id)
    {
        var venta = await _ventaService.GetByIdAsync(id);
        if (venta == null)
            return NotFound(new { mensaje = "Venta no encontrada." });
        return Ok(venta);
    }

    /// <summary>Registra una nueva venta con sus ítems.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VentaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VentaResponseDto>> Create([FromBody] VentaCreateDto dto)
    {
        try
        {
            // Obtener el ID del usuario autenticado desde el token JWT
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario autenticado." });
            }

            var result = await _ventaService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.IdVenta }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>Elimina una venta por su ID.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _ventaService.DeleteAsync(id);
        if (!result)
            return NotFound(new { mensaje = "Venta no encontrada." });
        return NoContent();
    }
}
