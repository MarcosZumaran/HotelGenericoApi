using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using HotelGenericoApi.Constants;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[EnableRateLimiting("authenticated")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class EstanciaController : ControllerBase
{
    private readonly IEstanciaQueryService _queryService;
    private readonly ICheckinService _checkinService;
    private readonly ICheckoutService _checkoutService;
    private readonly ISalidaTemporalService _salidaTemporalService;
    private readonly IHuespedService _huespedService;
    private readonly IConsumoEstanciaService _consumoService;
    private readonly ITrasladoHabitacionService _trasladoService;
    private readonly IReservaCommandService _reservaCommandService;
    private readonly IExcelExportService _excelExportService;
    private readonly IPdfService _pdfService;

    public EstanciaController(
        IEstanciaQueryService queryService,
        ICheckinService checkinService,
        ICheckoutService checkoutService,
        ISalidaTemporalService salidaTemporalService,
        IHuespedService huespedService,
        IConsumoEstanciaService consumoService,
        ITrasladoHabitacionService trasladoService,
        IReservaCommandService reservaCommandService,
        IExcelExportService excelExportService,
        IPdfService pdfService)
    {
        _queryService = queryService;
        _checkinService = checkinService;
        _checkoutService = checkoutService;
        _salidaTemporalService = salidaTemporalService;
        _huespedService = huespedService;
        _consumoService = consumoService;
        _trasladoService = trasladoService;
        _reservaCommandService = reservaCommandService;
        _excelExportService = excelExportService;
        _pdfService = pdfService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Estancia>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Estancia>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var estancias = await _queryService.GetPagedAsync(page, pageSize);
        return Ok(estancias);
    }

    [HttpGet("activas")]
    [ProducesResponseType(typeof(List<Estancia>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Estancia>>> GetActivas()
    {
        var activas = await _queryService.GetActivasAsync();
        return Ok(activas);
    }

    [HttpGet("activas/excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportActivasExcel()
    {
        var activas = await _queryService.GetActivasAsync();
        var bytes = _excelExportService.GenerateEstanciasActivasExcel(activas);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "huespedes_activos.xlsx");
    }

    [HttpGet("activas/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportActivasPdf()
    {
        var bytes = await _pdfService.GenerarPdfEstanciasActivasAsync();
        return File(bytes, "application/pdf", "huespedes_activos.pdf");
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Estancia), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Estancia>> GetById(int id)
    {
        var estancia = await _queryService.GetByIdAsync(id);
        if (estancia == null)
            return NotFound();
        return Ok(estancia);
    }

    [HttpPost("checkin")]
    [ProducesResponseType(typeof(Estancia), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Estancia>> Checkin([FromBody] CheckinCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _checkinService.CheckinAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.IdEstancia }, result);
    }

    [HttpPost("{id}/checkout")]
    [ProducesResponseType(typeof(CheckoutResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Checkout(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        try
        {
            var result = await _checkoutService.RealizarCheckoutAsync(id, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/salida-temporal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarSalidaTemporal(int id, [FromBody] SalidaTemporalDto dto)
    {
        try
        {
            await _salidaTemporalService.RegistrarSalidaTemporalAsync(id, dto.LlavesDejadas);
            return Ok(new { message = "Salida temporal registrada" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/regreso")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarRegreso(int id)
    {
        try
        {
            await _salidaTemporalService.RegistrarRegresoAsync(id);
            return Ok(new { message = "Regreso registrado" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/huespedes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AgregarHuesped(int id, [FromBody] AgregarHuespedDto dto)
    {
        try
        {
            var huesped = await _huespedService.AgregarHuespedCompletoAsync(id, dto);
            return Ok(new { idHuesped = huesped.IdHuesped, message = "Huésped agregado" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{idEstancia}/consumo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddConsumo(int idEstancia, [FromBody] ItemEstancia item)
    {
        var result = await _consumoService.AddConsumoAsync(idEstancia, item);
        if (!result)
            return BadRequest();
        return Ok();
    }

    [HttpPost("{idEstancia}/consumos/batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddConsumoBatch(int idEstancia, [FromBody] DTOs.Request.ConsumoBatchDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var items = dto.Items.Select(i => new ItemEstancia
        {
            IdProducto = i.IdProducto,
            Cantidad = i.Cantidad,
            PrecioUnitario = i.PrecioUnitario
        }).ToList();

        var result = await _consumoService.AddConsumoBatchAsync(idEstancia, items, userId);
        if (!result)
            return BadRequest();
        return Ok();
    }

    [HttpPost("{idEstancia}/consumos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddConsumos(int idEstancia, [FromBody] ConsumoListDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var items = dto.Consumos.Select(i => new ItemEstancia
        {
            IdProducto = i.IdProducto,
            Cantidad = i.Cantidad,
            PrecioUnitario = i.PrecioUnitario
        }).ToList();

        var result = await _consumoService.AddConsumoBatchAsync(idEstancia, items, userId);
        if (!result)
            return BadRequest();
        return Ok();
    }

    [HttpGet("{id}/consumos")]
    [ProducesResponseType(typeof(List<ItemConsumoResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ItemConsumoResponseDto>>> GetConsumos(int id)
    {
        var result = await _queryService.GetConsumosAsync(id);
        return Ok(result);
    }

    [HttpPut("{id}/consumo/{idItem}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateConsumo(int id, int idItem, [FromBody] ActualizarConsumoDto dto)
    {
        var result = await _consumoService.UpdateConsumoAsync(idItem, dto.Cantidad);
        if (!result)
            return NotFound();
        return Ok();
    }

    [HttpDelete("{id}/consumo/{idItem}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteConsumo(int id, int idItem)
    {
        var result = await _consumoService.DeleteConsumoAsync(idItem);
        if (!result)
            return NotFound();
        return Ok();
    }

    [HttpPost("{id}/trasladar")]
    [ProducesResponseType(typeof(TrasladoResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TrasladarHabitacion(int id, [FromBody] TrasladarEstanciaDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        try
        {
            var result = await _trasladoService.TrasladarHabitacionAsync(id, dto, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("reservas/{idHabitacion}")]
    [ProducesResponseType(typeof(List<ReservaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReservaResponseDto>>> GetReservasByHabitacion(int idHabitacion)
    {
        var result = await _queryService.GetReservasByHabitacionAsync(idHabitacion);
        return Ok(result);
    }

    [HttpPost("reserva")]
    [ProducesResponseType(typeof(Reserva), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Reserva>> CreateReserva([FromBody] ReservaCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _reservaCommandService.CreateReservaAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.IdReserva }, result);
    }

    [HttpPut("reserva/{id}/cancelar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelarReserva(int id)
    {
        var result = await _reservaCommandService.CancelarReservaAsync(id);
        if (!result)
            return NotFound();
        return Ok();
    }
}
