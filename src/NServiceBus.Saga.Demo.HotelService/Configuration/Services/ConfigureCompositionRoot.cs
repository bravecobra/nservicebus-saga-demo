﻿namespace NServiceBus.Saga.Demo.HotelService.Configuration.Services;

/// <summary>
/// 
/// </summary>
public static class ConfigureCompositionRoot
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCompositionRoot(this IServiceCollection services)
    {
        services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
        services.AddOptions();

        return services;
    }
}