namespace NServiceBus.Saga.Demo.TripService.Configuration.Services;

public static class ConfigureCompositionRoot
{
    public static IServiceCollection AddCompositionRoot(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
        services.AddOptions();
        services.AddCustomPersistence(configuration);
        return services;
    }
}