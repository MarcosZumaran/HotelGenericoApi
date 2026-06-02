namespace HotelGenericoApi.Modules.Recepcion;

public class RecepcionModule : IModule
{
    public string Name => "Recepcion";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Las dependencias se registran en ServiceExtensions.AddApplicationServices
    }


}
