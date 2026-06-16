using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IReservaCommandService
{
    Task<ReservaResponseDto> CreateReservaAsync(ReservaCreateDto dto, int idUsuario);
    Task<bool> CancelarReservaAsync(int idReserva);
}
