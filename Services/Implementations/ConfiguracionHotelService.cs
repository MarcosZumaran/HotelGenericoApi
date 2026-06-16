using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelGenericoApi.Services.Implementations;

public class ConfiguracionHotelService : IConfiguracionHotelService
{
    private readonly IConfiguracionCacheService _configCache;
    private readonly HotelDbContext _db;

    public ConfiguracionHotelService(IConfiguracionCacheService configCache, HotelDbContext db)
    {
        _configCache = configCache;
        _db = db;
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
                config.TasaIgvProductos,
                config.NombreComercial,
                config.CodigoEstablecimiento,
                config.PuntoEmisionBoleta,
                config.PuntoEmisionFactura,
                config.LogoUrl,
                config.Ubigeo,
                config.Departamento,
                config.Provincia,
                config.Distrito,
                config.Urbanizacion,
                config.AplicaExoneracionAmazonia,
                config.LeyendaAmazonia,
                config.RegimenTributario)
            : new ConfiguracionHotelResponseDto(
                "Hotel Genérico", null, null, null, 10.5m, 18.0m,
                null, null, null, null, null, null, null, null, null, null,
                false, null, null);
    }

    public async Task UpdateConfiguracionAsync(ConfiguracionGeneralUpdateDto dto)
    {
        var config = await _db.Configuraciones.FirstOrDefaultAsync();

        if (config == null)
        {
            config = new Models.Configuracion
            {
                Nombre = dto.Nombre ?? "Hotel Genérico",
                Direccion = dto.Direccion,
                Telefono = dto.Telefono,
                Ruc = dto.Ruc,
                TasaIgvHotel = dto.TasaIgvHotel ?? 10.5m,
                TasaIgvProductos = dto.TasaIgvProductos ?? 18.0m,
                NombreComercial = dto.NombreComercial,
                CodigoEstablecimiento = dto.CodigoEstablecimiento ?? "0000",
                PuntoEmisionBoleta = dto.PuntoEmisionBoleta ?? "001",
                PuntoEmisionFactura = dto.PuntoEmisionFactura ?? "001",
                LogoUrl = dto.LogoUrl,
                Ubigeo = dto.Ubigeo,
                Departamento = dto.Departamento,
                Provincia = dto.Provincia,
                Distrito = dto.Distrito,
                Urbanizacion = dto.Urbanizacion,
                AplicaExoneracionAmazonia = dto.AplicaExoneracionAmazonia ?? false,
                LeyendaAmazonia = dto.LeyendaAmazonia,
                RegimenTributario = dto.RegimenTributario,
                FechaActualizacion = DateTime.UtcNow,
            };
            _db.Configuraciones.Add(config);
        }
        else
        {
            if (dto.Nombre != null) config.Nombre = dto.Nombre;
            if (dto.Direccion != null) config.Direccion = dto.Direccion;
            if (dto.Telefono != null) config.Telefono = dto.Telefono;
            if (dto.Ruc != null) config.Ruc = dto.Ruc;
            if (dto.TasaIgvHotel != null) config.TasaIgvHotel = dto.TasaIgvHotel.Value;
            if (dto.TasaIgvProductos != null) config.TasaIgvProductos = dto.TasaIgvProductos.Value;
            if (dto.NombreComercial != null) config.NombreComercial = dto.NombreComercial;
            if (dto.CodigoEstablecimiento != null) config.CodigoEstablecimiento = dto.CodigoEstablecimiento;
            if (dto.PuntoEmisionBoleta != null) config.PuntoEmisionBoleta = dto.PuntoEmisionBoleta;
            if (dto.PuntoEmisionFactura != null) config.PuntoEmisionFactura = dto.PuntoEmisionFactura;
            if (dto.LogoUrl != null) config.LogoUrl = dto.LogoUrl;
            if (dto.Ubigeo != null) config.Ubigeo = dto.Ubigeo;
            if (dto.Departamento != null) config.Departamento = dto.Departamento;
            if (dto.Provincia != null) config.Provincia = dto.Provincia;
            if (dto.Distrito != null) config.Distrito = dto.Distrito;
            if (dto.Urbanizacion != null) config.Urbanizacion = dto.Urbanizacion;
            if (dto.AplicaExoneracionAmazonia != null) config.AplicaExoneracionAmazonia = dto.AplicaExoneracionAmazonia.Value;
            if (dto.LeyendaAmazonia != null) config.LeyendaAmazonia = dto.LeyendaAmazonia;
            if (dto.RegimenTributario != null) config.RegimenTributario = dto.RegimenTributario;
            config.FechaActualizacion = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        _configCache.InvalidateCache();
    }

    public async Task<string> UpdateLogoAsync(string fileName)
    {
        var config = await _db.Configuraciones.FirstOrDefaultAsync();

        if (config == null)
        {
            config = new Models.Configuracion
            {
                Nombre = "Hotel Genérico",
                LogoUrl = fileName,
                FechaActualizacion = DateTime.UtcNow,
            };
            _db.Configuraciones.Add(config);
        }
        else
        {
            config.LogoUrl = fileName;
            config.FechaActualizacion = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        _configCache.InvalidateCache();
        return fileName;
    }
}
