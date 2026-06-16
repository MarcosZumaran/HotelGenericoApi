using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Extensions;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[EnableRateLimiting("authenticated")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class HabitacionController : ControllerBase
{
    private readonly IHabitacionService _habitacionService;
    private readonly IAmenidadService _amenidadService;

    public HabitacionController(IHabitacionService habitacionService, IAmenidadService amenidadService)
    {
        _habitacionService = habitacionService;
        _amenidadService = amenidadService;
    }

    private static HabitacionResponseDto ToDto(Habitacion h)
    {
        Dictionary<string, bool>? caracteristicas = null;
        if (!string.IsNullOrEmpty(h.Caracteristicas))
        {
            try { caracteristicas = JsonSerializer.Deserialize<Dictionary<string, bool>>(h.Caracteristicas); } catch { }
        }

        return new HabitacionResponseDto(
            IdHabitacion: h.IdHabitacion,
            NumeroHabitacion: h.NumeroHabitacion,
            Piso: h.Piso,
            Descripcion: h.Descripcion,
            IdTipo: h.IdTipo,
            NombreTipo: h.Tipo?.Nombre ?? "",
            PrecioNoche: h.PrecioNoche,
            IdEstado: h.IdEstado,
            NombreEstado: h.Estado ?? "",
            FechaUltimoCambio: h.FechaUltimoCambio,
            UsuarioCambio: h.UsuarioCambio,
            Caracteristicas: caracteristicas,
            Amenidades: h.HabitacionAmenidades?.Select(ha => new HabitacionAmenidadResponseDto
            {
                IdProducto = ha.IdProducto,
                NombreProducto = ha.IdProductoNavigation?.Nombre ?? "",
                CantidadBase = ha.CantidadBase
            }).ToList()
        );
    }

    /// <summary>Obtiene todas las habitaciones registradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Habitacion>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Habitacion>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var habitaciones = await _habitacionService.GetPagedAsync(page, pageSize);
        return Ok(habitaciones);
    }

    /// <summary>Obtiene una habitación por su ID.</summary>
    /// <param name="id">ID de la habitación.</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Habitacion), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Habitacion>> GetById(int id)
    {
        var habitacion = await _habitacionService.GetByIdAsync(id);
        if (habitacion == null)
            return NotFound();
        return Ok(habitacion);
    }

    /// <summary>Crea una nueva habitación.</summary>
    /// <param name="dto">Datos de la habitación.</param>
    [HttpPost]
    [ProducesResponseType(typeof(HabitacionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HabitacionResponseDto>> Create([FromBody] HabitacionCreateDto dto)
    {
        var result = await _habitacionService.CreateAsync(dto);
        var full = await _habitacionService.GetByIdAsync(result.IdHabitacion);
        if (full == null)
            return CreatedAtAction(nameof(GetById), new { id = result.IdHabitacion }, ToDto(result));
        return CreatedAtAction(nameof(GetById), new { id = result.IdHabitacion }, ToDto(full));
    }

    /// <summary>Actualiza los datos de una habitación existente.</summary>
    /// <param name="id">ID de la habitación.</param>
    /// <param name="dto">Datos actualizados.</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(HabitacionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HabitacionResponseDto>> Update(int id, [FromBody] HabitacionUpdateDto dto)
    {
        await _habitacionService.UpdateAsync(id, dto);
        var full = await _habitacionService.GetByIdAsync(id);
        if (full == null)
            return NotFound();
        return Ok(ToDto(full));
    }

    /// <summary>Elimina una habitación por su ID.</summary>
    /// <param name="id">ID de la habitación.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _habitacionService.DeleteAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    /// <summary>Obtiene las habitaciones disponibles en un rango de fechas.</summary>
    [HttpGet("disponibles")]
    [ProducesResponseType(typeof(List<HabitacionEstadoActualDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HabitacionEstadoActualDto>>> GetDisponibles(
        [FromQuery] DateTime fechaEntrada,
        [FromQuery] DateTime fechaSalida)
    {
        var result = await _habitacionService.GetDisponiblesAsync(fechaEntrada, fechaSalida);
        return Ok(result);
    }

    /// <summary>Obtiene el estado actual de todas las habitaciones con datos en tiempo real.</summary>
    [HttpGet("estado-actual")]
    [ProducesResponseType(typeof(List<HabitacionEstadoActualDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HabitacionEstadoActualDto>>> GetEstadoActual()
    {
        var result = await _habitacionService.GetEstadoActualAsync();
        return Ok(result);
    }

    /// <summary>Parchea una habitación: cambia estado o actualiza datos según el body.</summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Patch(int id, [FromBody] HabitacionPatchDto dto)
    {
        if (dto.IdEstado.HasValue)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var ok = await _habitacionService.CambiarEstadoAsync(id, dto.IdEstado.Value, userId);
            if (!ok) return NotFound();
            return NoContent();
        }

        var updateDto = new HabitacionUpdateDto
        {
            NumeroHabitacion = dto.NumeroHabitacion,
            Piso = dto.Piso,
            Descripcion = dto.Descripcion,
            IdTipo = dto.IdTipo,
            PrecioNoche = dto.PrecioNoche,
        };
        await _habitacionService.UpdateAsync(id, updateDto);
        var full = await _habitacionService.GetByIdAsync(id);
        if (full == null) return NotFound();
        return Ok(ToDto(full));
    }

    /// <summary>Cambia el estado de una habitación validando transiciones permitidas.</summary>
    [HttpPatch("{idHabitacion}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CambiarEstado(int idHabitacion, [FromQuery] int idNuevoEstado, [FromQuery] int idUsuario, [FromQuery] string? observacion = null)
    {
        var result = await _habitacionService.CambiarEstadoAsync(idHabitacion, idNuevoEstado, idUsuario, observacion);
        if (!result)
            return NotFound();
        return NoContent();
    }

    /// <summary>Sugiere una habitación disponible según criterios opcionales.</summary>
    [HttpGet("sugerir-disponible")]
    [ProducesResponseType(typeof(HabitacionSugeridaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<HabitacionSugeridaDto>> SugerirDisponible(
        [FromQuery] int? tipoHabitacion = null,
        [FromQuery] int? piso = null,
        [FromQuery] int? cercanaA = null)
    {
        var result = await _habitacionService.SugerirDisponibleAsync(tipoHabitacion, piso, cercanaA);
        if (result == null)
            return NoContent();
        return Ok(result);
    }

    /// <summary>Obtiene el estado actual detallado con prioridad de limpieza y minutos en estado.</summary>
    [HttpGet("estado-actual-detallado")]
    [ProducesResponseType(typeof(List<HabitacionEstadoActualDetalladoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HabitacionEstadoActualDetalladoDto>>> GetEstadoActualDetallado()
    {
        var result = await _habitacionService.GetEstadoActualDetalladoAsync();
        return Ok(result);
    }

    /// <summary>Obtiene el estado actual de las amenities de una habitación para previsualizar reposición.</summary>
    [HttpGet("{id}/amenidades-estado")]
    [ProducesResponseType(typeof(List<AmenidadEstadoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AmenidadEstadoDto>>> GetAmenidadesEstado(int id)
    {
        var estado = await _amenidadService.GetAmenidadesEstadoAsync(id);
        return Ok(estado);
    }

    /// <summary>Obtiene las amenidades personalizadas de una habitación.</summary>
    [HttpGet("{id}/amenidades")]
    public async Task<IActionResult> GetAmenidades(int id)
    {
        var amenidades = await _habitacionService.GetAmenidadesPorHabitacionAsync(id);
        var dto = amenidades.Select(a => new { a.IdProducto, a.CantidadBase, a.Producto?.Nombre });
        return Ok(dto);
    }

    /// <summary>Actualiza las amenidades personalizadas de una habitación.</summary>
    [HttpPut("{id}/amenidades")]
    public async Task<IActionResult> UpdateAmenidades(int id, [FromBody] List<HabitacionAmenidadDto> dto)
    {
        var result = await _habitacionService.ActualizarAmenidadesAsync(id, dto);
        if (!result) return NotFound();
        return Ok();
    }

    /// <summary>Obtiene las características extra de una habitación.</summary>
    [HttpGet("{id}/caracteristicas")]
    public async Task<IActionResult> GetCaracteristicas(int id)
    {
        var caracteristicas = await _habitacionService.GetCaracteristicasAsync(id);
        return Ok(caracteristicas ?? new Dictionary<string, bool>());
    }

    /// <summary>Actualiza las características extra de una habitación.</summary>
    [HttpPut("{id}/caracteristicas")]
    public async Task<IActionResult> UpdateCaracteristicas(int id, [FromBody] Dictionary<string, bool> caracteristicas)
    {
        var result = await _habitacionService.ActualizarCaracteristicasAsync(id, caracteristicas);
        if (!result) return NotFound();
        return Ok();
    }
}
