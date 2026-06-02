using System.Reflection;

namespace HotelGenericoApi.Modules;

public static class ModuleRegistrar
{
    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
    {
        var modules = DiscoverModules();
        foreach (var module in modules)
        {
            module.RegisterServices(services, configuration);
        }
        services.AddSingleton(modules);
        return services;
    }

    private static List<IModule> DiscoverModules()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .Select(Activator.CreateInstance)
            .Cast<IModule>()
            .ToList();
    }
}
