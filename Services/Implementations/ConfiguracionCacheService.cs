using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using HotelGenericoApi.Data;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class ConfiguracionCacheService : IConfiguracionCacheService
{
    private readonly IMemoryCache _cache;
    private readonly HotelDbContext _db;
    private const string CacheKey = "ConfiguracionGlobal";

    public ConfiguracionCacheService(IMemoryCache cache, HotelDbContext db)
    {
        _cache = cache;
        _db = db;
    }

    public async Task<Configuracion?> GetConfiguracionAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(3);
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _db.Configuraciones.AsNoTracking().FirstOrDefaultAsync();
        });
    }

    public void InvalidateCache()
    {
        _cache.Remove(CacheKey);
    }
}
