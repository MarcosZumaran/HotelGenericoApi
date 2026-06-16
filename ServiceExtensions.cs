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
        services.AddScoped<IEstanciaQueryService, EstanciaQueryService>();
        services.AddScoped<ICheckinService, CheckinService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<ISalidaTemporalService, SalidaTemporalService>();
        services.AddScoped<IHuespedService, HuespedService>();
        services.AddScoped<IConsumoEstanciaService, ConsumoEstanciaService>();
        services.AddScoped<ITrasladoHabitacionService, TrasladoHabitacionService>();
        services.AddScoped<IReservaQueryService, ReservaQueryService>();
        services.AddScoped<IReservaCommandService, ReservaCommandService>();
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
		services.AddScoped<IIncidenteService, IncidenteService>();
        services.AddScoped<IStockHabitacionService, StockHabitacionService>();

        services.AddScoped<IParametroHotelService, ParametroHotelService>();
        services.AddScoped<IFolioService, FolioService>();

		// HttpClient tipificado para RENIEC
        services.AddHttpClient<IReniecService, ReniecService>();

        // Caché
        services.AddMemoryCache();
        services.AddScoped<IConfiguracionCacheService, ConfiguracionCacheService>();

        // Setup y transacciones
        services.AddScoped<SetupService>();
        services.AddScoped<IDbTransactionManager, SqlServerTransactionManager>();

        // Backup
        services.AddScoped<IBackupService, BackupService>();

        // Excel
        services.AddScoped<IExcelExportService, ExcelExportService>();

        // Facturacion XML
        services.AddScoped<IXmlComprobanteService, XmlComprobanteService>();

        // limpiado de backups
        services.AddHostedService<BackupCleanupService>();

        return services;
    }
}
