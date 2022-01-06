﻿using Serilog;
using Serilog.Events;

namespace NServiceBus.Saga.Demo.TripService.Configuration.Services;

public static class ConfigureLogging
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="loggerConfigurationAction"></param>
    /// <returns></returns>
    public static IHostBuilder UseCustomLogging(this IHostBuilder hostBuilder, Action<LoggerConfiguration>? loggerConfigurationAction = null)
    {
        return hostBuilder.UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Override("Serilog", LogEventLevel.Warning)
                .MinimumLevel.Override("CorrelationId", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                ;
            loggerConfigurationAction?.Invoke(loggerConfiguration);
        });
    }
}