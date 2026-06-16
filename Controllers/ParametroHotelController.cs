using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ParametroHotelController : ControllerBase
{
    private readonly IParametroHotelService _parametroHotelService;

    public ParametroHotelController(IParametroHotelService parametroHotelService)
    {
        _parametroHotelService = parametroHotelService;
    }

    [HttpGet("limpieza")]
    [ProducesResponseType(typeof(LimpiezaParamsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LimpiezaParamsDto>> GetLimpieza()
    {
        var result = await _parametroHotelService.GetLimpiezaParamsAsync();
        return Ok(result);
    }

    [HttpPut("limpieza")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateLimpieza([FromBody] LimpiezaParamsUpdateDto dto)
    {
        if (!HasAnyValue(dto))
            return BadRequest(new { mensaje = "Debe enviar al menos un parámetro para actualizar" });

        var error = Validate(dto);
        if (error != null)
            return BadRequest(new { mensaje = error });

        await _parametroHotelService.UpdateLimpiezaParamsAsync(dto);
        return NoContent();
    }

    private static bool HasAnyValue(LimpiezaParamsUpdateDto dto)
    {
        return typeof(LimpiezaParamsUpdateDto).GetProperties()
            .Any(p => p.GetValue(dto) != null);
    }

    [HttpGet("checkout")]
    [ProducesResponseType(typeof(CheckoutParamsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CheckoutParamsDto>> GetCheckout()
    {
        var result = await _parametroHotelService.GetCheckoutParamsAsync();
        return Ok(result);
    }

    [HttpPut("checkout")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateCheckout([FromBody] CheckoutParamsUpdateDto dto)
    {
        if (!HasAnyValueCheckout(dto))
            return BadRequest(new { mensaje = "Debe enviar al menos un parámetro para actualizar" });

        var error = ValidateCheckout(dto);
        if (error != null)
            return BadRequest(new { mensaje = error });

        await _parametroHotelService.UpdateCheckoutParamsAsync(dto);
        return NoContent();
    }

    private static bool HasAnyValueCheckout(CheckoutParamsUpdateDto dto)
    {
        return typeof(CheckoutParamsUpdateDto).GetProperties()
            .Any(p => p.GetValue(dto) != null);
    }

    private static string? ValidateCheckout(CheckoutParamsUpdateDto dto)
    {
        if (dto.CheckoutHoraLimite != null)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.CheckoutHoraLimite, @"^\d{2}:\d{2}$"))
                return "checkout_hora_limite debe tener formato HH:mm";
        }
        if (dto.CheckoutCargoPorHora != null)
        {
            if (!decimal.TryParse(dto.CheckoutCargoPorHora, out var val) || val < 0)
                return "checkout_cargo_por_hora debe ser un número decimal positivo";
        }
        if (dto.CheckoutGraciaMinutos != null)
        {
            if (!int.TryParse(dto.CheckoutGraciaMinutos, out var val) || val < 0)
                return "checkout_gracia_minutos debe ser un número entero no negativo";
        }
        return null;
    }

    [HttpGet("pagos")]
    [ProducesResponseType(typeof(PagosParamsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagosParamsDto>> GetPagos()
    {
        var result = await _parametroHotelService.GetPagosParamsAsync();
        return Ok(result);
    }

    [HttpPut("pagos")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdatePagos([FromBody] PagosParamsUpdateDto dto)
    {
        if (!HasAnyValuePagos(dto))
            return BadRequest(new { mensaje = "Debe enviar al menos un parámetro para actualizar" });

        await _parametroHotelService.UpdatePagosParamsAsync(dto);
        return NoContent();
    }

    [HttpGet("notificaciones")]
    [ProducesResponseType(typeof(NotificacionesParamsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificacionesParamsDto>> GetNotificaciones()
    {
        var result = await _parametroHotelService.GetNotificacionesParamsAsync();
        return Ok(result);
    }

    [HttpPut("notificaciones")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateNotificaciones([FromBody] NotificacionesParamsUpdateDto dto)
    {
        if (!HasAnyValueNotificaciones(dto))
            return BadRequest(new { mensaje = "Debe enviar al menos un parámetro para actualizar" });

        await _parametroHotelService.UpdateNotificacionesParamsAsync(dto);
        return NoContent();
    }

    [HttpGet("deposito-garantia")]
    [ProducesResponseType(typeof(DepositoGarantiaParamsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DepositoGarantiaParamsDto>> GetDepositoGarantia()
    {
        var result = await _parametroHotelService.GetDepositoGarantiaParamsAsync();
        return Ok(result);
    }

    [HttpPut("deposito-garantia")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateDepositoGarantia([FromBody] DepositoGarantiaParamsUpdateDto dto)
    {
        if (!HasAnyValueDepositoGarantia(dto))
            return BadRequest(new { mensaje = "Debe enviar al menos un parámetro para actualizar" });
        await _parametroHotelService.UpdateDepositoGarantiaParamsAsync(dto);
        return NoContent();
    }

    [HttpGet("early-checkin")]
    [ProducesResponseType(typeof(EarlyCheckinParamsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EarlyCheckinParamsDto>> GetEarlyCheckin()
    {
        var result = await _parametroHotelService.GetEarlyCheckinParamsAsync();
        return Ok(result);
    }

    [HttpPut("early-checkin")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateEarlyCheckin([FromBody] EarlyCheckinParamsUpdateDto dto)
    {
        if (!HasAnyValueEarlyCheckin(dto))
            return BadRequest(new { mensaje = "Debe enviar al menos un parámetro para actualizar" });
        await _parametroHotelService.UpdateEarlyCheckinParamsAsync(dto);
        return NoContent();
    }

    private static bool HasAnyValuePagos(PagosParamsUpdateDto dto)
    {
        return typeof(PagosParamsUpdateDto).GetProperties()
            .Any(p => p.GetValue(dto) != null);
    }

    private static bool HasAnyValueNotificaciones(NotificacionesParamsUpdateDto dto)
    {
        return typeof(NotificacionesParamsUpdateDto).GetProperties()
            .Any(p => p.GetValue(dto) != null);
    }

    private static bool HasAnyValueDepositoGarantia(DepositoGarantiaParamsUpdateDto dto)
    {
        return typeof(DepositoGarantiaParamsUpdateDto).GetProperties()
            .Any(p => p.GetValue(dto) != null);
    }

    private static bool HasAnyValueEarlyCheckin(EarlyCheckinParamsUpdateDto dto)
    {
        return typeof(EarlyCheckinParamsUpdateDto).GetProperties()
            .Any(p => p.GetValue(dto) != null);
    }

    private static string? Validate(LimpiezaParamsUpdateDto dto)
    {
        if (dto.LimpiezaSalidaTiempo != null)
        {
            if (!int.TryParse(dto.LimpiezaSalidaTiempo, out var val) || val <= 0)
                return "limpieza_salida_tiempo debe ser un número entero positivo";
        }
        if (dto.LimpiezaEstanciaTiempo != null)
        {
            if (!int.TryParse(dto.LimpiezaEstanciaTiempo, out var val) || val <= 0)
                return "limpieza_estancia_tiempo debe ser un número entero positivo";
        }
        if (dto.LimpiezaFrecuenciaHoras != null)
        {
            if (!int.TryParse(dto.LimpiezaFrecuenciaHoras, out var val) || val <= 0)
                return "limpieza_frecuencia_horas debe ser un número entero positivo";
        }
        if (dto.LimpiezaHorarioInicio != null)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.LimpiezaHorarioInicio, @"^\d{2}:\d{2}$"))
                return "limpieza_horario_inicio debe tener formato HH:mm";
        }
        if (dto.LimpiezaHorarioFin != null)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.LimpiezaHorarioFin, @"^\d{2}:\d{2}$"))
                return "limpieza_horario_fin debe tener formato HH:mm";
        }
        return null;
    }
}
