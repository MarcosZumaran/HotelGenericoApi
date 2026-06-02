using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface ICheckoutService
{
    Task<CheckoutResultDto> RealizarCheckoutAsync(int estanciaId, int idUsuario);
}
