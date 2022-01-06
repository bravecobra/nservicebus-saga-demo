using Serilog;
using Serilog.Events;

namespace NServiceBus.Saga.Demo.TransportationService.Configuration.Services
{
    public static class ConfigureLogging
    {
        public static IHostBuilder UseCustomLogging(this IHostBuilder hostBuilder, Action<LoggerConfiguration>? loggerConfigurationAction = null)
        {
            return hostBuilder.UseSerilog((hostingContext, loggerConfiguration) =>
            {
                loggerConfiguration
                    .MinimumLevel.Information()
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
}