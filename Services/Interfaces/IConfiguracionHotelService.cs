using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IConfiguracionHotelService
{
    Task<ConfiguracionHotelResponseDto> GetConfiguracionAsync();
    Task UpdateConfiguracionAsync(ConfiguracionGeneralUpdateDto dto);
    Task<string> UpdateLogoAsync(string fileName);
}
