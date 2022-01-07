using NServiceBus.Saga.Demo.Contracts.Hotels;

namespace NServiceBus.Saga.Demo.HotelService.Configuration.Services;

public static class ConfigureEventBusExtensions
{
    public static IHostBuilder UseCustomEventBus(this IHostBuilder builder)
    {
        builder.UseNServiceBus(context =>
        {
            var endpointConfiguration = new EndpointConfiguration("HotelService");

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>()
                .ConnectionString(context.Configuration.GetConnectionString("RabbitMq"))
                .UseConventionalRoutingTopology();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.EnableOutbox();
            endpointConfiguration.MakeInstanceUniquelyAddressable(Environment.MachineName);
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.SendFailedMessagesTo(errorQueue: "error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.AuditSagaStateChanges(serviceControlQueue: "audit");

            endpointConfiguration.EnableMetrics().SendMetricDataToServiceControl(
                serviceControlMetricsAddress: "Particular.Monitoring",
                interval: TimeSpan.FromSeconds(2)
            );
            endpointConfiguration.ConnectToServicePlatform(context.Configuration.GetSection("NServiceBus").Get<ServicePlatformConnectionConfiguration>());

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(HotelBooked), "TripService");
            return endpointConfiguration;
        });
        return builder;
    }
}