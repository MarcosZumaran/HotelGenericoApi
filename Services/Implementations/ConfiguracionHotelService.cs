using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelGenericoApi.Services.Implementations;

public class ConfiguracionHotelService : IConfiguracionHotelService
{
    private readonly IConfiguracionCacheService _configCache;

    public ConfiguracionHotelService(IConfiguracionCacheService configCache)
    {
        _configCache = configCache;
    }

    public async Task<ConfiguracionHotelResponseDto> GetConfiguracionAsync()
    {
        var config = await _configCache.GetConfiguracionAsync();

        return config is not null
            ? new ConfiguracionHotelResponseDto(
                config.Nombre,
                config.Direccion,
                config.Telefono,
                config.Ruc,
                config.TasaIgvHotel,
                config.TasaIgvProductos)
            : new ConfiguracionHotelResponseDto("Hotel Genérico", null, null, null, 10.5m, 18.0m);
    }
}