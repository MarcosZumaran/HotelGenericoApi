using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("authenticated")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class EstanciaController : ControllerBase
{
    private readonly IEstanciaService _estanciaService;

    public EstanciaController(IEstanciaService estanciaService)
    {
        _estanciaService = estanciaService;
    }

    /// <summary>Obtiene todas las estancias registradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Estancia>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Estancia>>> GetAll()
    {
        var estancias = await _estanciaService.GetAllAsync();
        return Ok(estancias);
    }

    /// <summary>Obtiene una estancia por su ID con detalles completos.</summary>
    /// <param name="id">ID de la estancia.</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Estancia), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Estancia>> GetById(int id)
    {
        var estancia = await _estanciaService.GetByIdAsync(id);
        if (estancia == null)
            return NotFound();
        return Ok(estancia);
    }

    /// <summary>Registra un check-in creando una nueva estancia y marcando la habitación como ocupada.</summary>
    /// <param name="estancia">Datos de la estancia.</param>
    [HttpPost]
    [ProducesResponseType(typeof(Estancia), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Estancia>> Create([FromBody] Estancia estancia)
    {
        var result = await _estanciaService.CreateAsync(estancia);
        return CreatedAtAction(nameof(GetById), new { id = result.IdEstancia }, result);
    }

    /// <summary>Registra el check-out de una estancia, liberando la habitación a limpieza.</summary>
    [HttpPost("{idEstancia}/checkout")]
    [ProducesResponseType(typeof(Estancia), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Estancia>> Checkout(int idEstancia, [FromQuery] int? idUsuario = null)
    {
        if (!idUsuario.HasValue)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();
            idUsuario = userId;
        }

        var result = await _estanciaService.CheckoutAsync(idEstancia, idUsuario.Value);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Añade un huésped adicional a una estancia existente.</summary>
    /// <param name="idEstancia">ID de la estancia.</param>
    /// <param name="huesped">Datos del huésped.</param>
    [HttpPost("{idEstancia}/huesped")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddHuesped(int idEstancia, [FromBody] Huesped huesped)
    {
        var result = await _estanciaService.AddHuespedAsync(idEstancia, huesped);
        if (!result)
            return BadRequest();
        return Ok();
    }

    /// <summary>Registra un consumo (producto) en una estancia activa.</summary>
    [HttpPost("{idEstancia}/consumo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddConsumo(int idEstancia, [FromBody] ItemEstancia item)
    {
        var result = await _estanciaService.AddConsumoAsync(idEstancia, item);
        if (!result)
            return BadRequest();
        return Ok();
    }

    /// <summary>Obtiene los consumos de una estancia.</summary>
    [HttpGet("{id}/consumos")]
    [ProducesResponseType(typeof(List<ItemConsumoResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ItemConsumoResponseDto>>> GetConsumos(int id)
    {
        var result = await _estanciaService.GetConsumosAsync(id);
        return Ok(result);
    }

    /// <summary>Actualiza un consumo en una estancia.</summary>
    [HttpPut("{id}/consumo/{idItem}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateConsumo(int id, int idItem, [FromBody] ActualizarConsumoDto dto)
    {
        var result = await _estanciaService.UpdateConsumoAsync(idItem, dto.Cantidad);
        if (!result)
            return NotFound();
        return Ok();
    }

    /// <summary>Elimina un consumo de una estancia.</summary>
    [HttpDelete("{id}/consumo/{idItem}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteConsumo(int id, int idItem)
    {
        var result = await _estanciaService.DeleteConsumoAsync(idItem);
        if (!result)
            return NotFound();
        return Ok();
    }

    /// <summary>Obtiene las reservas de una habitación.</summary>
    [HttpGet("reservas/{idHabitacion}")]
    [ProducesResponseType(typeof(List<ReservaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReservaResponseDto>>> GetReservasByHabitacion(int idHabitacion)
    {
        var result = await _estanciaService.GetReservasByHabitacionAsync(idHabitacion);
        return Ok(result);
    }

    /// <summary>Realiza check-in creando una estancia y ocupando la habitación.</summary>
    [HttpPost("checkin")]
    [ProducesResponseType(typeof(Estancia), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Estancia>> Checkin([FromBody] CheckinCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _estanciaService.CheckinAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.IdEstancia }, result);
    }

    /// <summary>Crea una nueva reserva.</summary>
    [HttpPost("reserva")]
    [ProducesResponseType(typeof(Reserva), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Reserva>> CreateReserva([FromBody] ReservaCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _estanciaService.CreateReservaAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.IdReserva }, result);
    }

    /// <summary>Registra la salida temporal de un huésped (deja la habitación pero no hace checkout).</summary>
    [HttpPost("{id}/salida-temporal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarSalidaTemporal(int id, [FromBody] SalidaTemporalDto dto)
    {
        try
        {
            await _estanciaService.RegistrarSalidaTemporalAsync(id, dto.LlavesDejadas);
            return Ok(new { message = "Salida temporal registrada" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Registra el regreso de un huésped que estaba fuera temporalmente.</summary>
    [HttpPost("{id}/regreso")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarRegreso(int id)
    {
        try
        {
            await _estanciaService.RegistrarRegresoAsync(id);
            return Ok(new { message = "Regreso registrado" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Agrega un huésped adicional a una estancia (crea el cliente si no existe).</summary>
    [HttpPost("{id}/huespedes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AgregarHuesped(int id, [FromBody] AgregarHuespedDto dto)
    {
        try
        {
            var huesped = await _estanciaService.AgregarHuespedCompletoAsync(id, dto);
            return Ok(new { idHuesped = huesped.IdHuesped, message = "Huésped agregado" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Realiza el checkout completo (suma consumos, genera comprobante y libera habitación).</summary>
    [HttpPost("{id}/checkout-completo")]
    [ProducesResponseType(typeof(CheckoutResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckoutCompleto(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        try
        {
            var result = await _estanciaService.RealizarCheckoutCompletoAsync(id, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Cancela una reserva existente.</summary>
    [HttpPut("reserva/{id}/cancelar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelarReserva(int id)
    {
        var result = await _estanciaService.CancelarReservaAsync(id);
        if (!result)
            return NotFound();
        return Ok();
    }
}
