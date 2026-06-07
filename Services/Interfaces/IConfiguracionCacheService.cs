using HotelGenericoApi.Models;

namespace HotelGenericoApi.Services.Interfaces;

public interface IConfiguracionCacheService
{
    Task<Configuracion?> GetConfiguracionAsync();
    void InvalidateCache();
}
