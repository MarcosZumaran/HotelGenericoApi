using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface ITrasladoHabitacionService
{
    Task<TrasladoResultDto> TrasladarHabitacionAsync(int estanciaId, TrasladarEstanciaDto dto, int idUsuario);
}
