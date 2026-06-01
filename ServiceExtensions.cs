using HotelGenericoApi.Data;
using HotelGenericoApi.Hubs;
using HotelGenericoApi.Mappings;
using HotelGenericoApi.Services.Implementations;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, ConfigurationManager configuration)
    {
        // NLua
        services.AddSingleton<ILuaService, LuaService>();

        // Mappers
        services.AddSingleton<EstadoHabitacionMapper>();
        services.AddSingleton<RolUsuarioMapper>();
        services.AddSingleton<MetodoPagoMapper>();
        services.AddSingleton<TipoDocumentoMapper>();
        services.AddSingleton<TipoComprobanteMapper>();
        services.AddSingleton<AfectacionIgvMapper>();
        services.AddSingleton<EstadoSunatMapper>();
        services.AddSingleton<TiposHabitacionMapper>();
        services.AddSingleton<UsuarioMapper>();
        services.AddSingleton<ClienteMapper>();
        services.AddSingleton<HabitacionMapper>();
        services.AddSingleton<ProductoMapper>();

        // Servicios
        services.AddScoped<ICatEstadoHabitacionService, CatEstadoHabitacionService>();
        services.AddScoped<ICatRolUsuarioService, CatRolUsuarioService>();
        services.AddScoped<ICatMetodoPagoService, CatMetodoPagoService>();
        services.AddScoped<ICatTipoDocumentoService, CatTipoDocumentoService>();
        services.AddScoped<ICatTipoComprobanteService, CatTipoComprobanteService>();
        services.AddScoped<ICatAfectacionIgvService, CatAfectacionIgvService>();
        services.AddScoped<ICatEstadoSunatService, CatEstadoSunatService>();
        services.AddScoped<ITiposHabitacionService, TiposHabitacionService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IHabitacionService, HabitacionService>();
        services.AddScoped<IEstanciaService, EstanciaService>();
        services.AddScoped<IReservaService, ReservaService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IComprobanteService, ComprobanteService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<IVentaService, VentaService>();
        services.AddScoped<ICierreCajaEnvioService, CierreCajaEnvioService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IConfiguracionHotelService, ConfiguracionHotelService>();
        services.AddScoped<IValidadorEstadoService, ValidadorEstadoService>();
        services.AddScoped<ICategoriaProductoService, CategoriaProductoService>();
        services.AddScoped<IAmenidadService, AmenidadService>();
        services.AddScoped<IReservaCorporativaService, ReservaCorporativaService>();

        // HttpClient tipificado para RENIEC
        services.AddHttpClient<IReniecService, ReniecService>();

        // Setup y transacciones
        services.AddScoped<SetupService>();
        services.AddScoped<IDbTransactionManager, SqlServerTransactionManager>();

        // Backup
        services.AddScoped<IBackupService, BackupService>();

        // limpiado de backups
        services.AddHostedService<BackupCleanupService>();

        return services;
    }
}
