using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IParametroHotelService
{
    Task<LimpiezaParamsDto> GetLimpiezaParamsAsync();
    Task UpdateLimpiezaParamsAsync(LimpiezaParamsUpdateDto dto);
    Task<CheckoutParamsDto> GetCheckoutParamsAsync();
    Task UpdateCheckoutParamsAsync(CheckoutParamsUpdateDto dto);
    Task<PagosParamsDto> GetPagosParamsAsync();
    Task UpdatePagosParamsAsync(PagosParamsUpdateDto dto);
    Task<NotificacionesParamsDto> GetNotificacionesParamsAsync();
    Task UpdateNotificacionesParamsAsync(NotificacionesParamsUpdateDto dto);
    Task<DepositoGarantiaParamsDto> GetDepositoGarantiaParamsAsync();
    Task UpdateDepositoGarantiaParamsAsync(DepositoGarantiaParamsUpdateDto dto);
    Task<EarlyCheckinParamsDto> GetEarlyCheckinParamsAsync();
    Task UpdateEarlyCheckinParamsAsync(EarlyCheckinParamsUpdateDto dto);
}
