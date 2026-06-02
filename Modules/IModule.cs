namespace HotelGenericoApi.Modules;

public interface IModule
{
    string Name { get; }
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
}
