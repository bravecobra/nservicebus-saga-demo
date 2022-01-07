using NServiceBus.Common;
using NServiceBus.Saga.Demo.Contracts.Flights;
using NServiceBus.Saga.Demo.Contracts.Hotels;
using NServiceBus.Saga.Demo.Contracts.Trips;

namespace NServiceBus.Saga.Demo.TripService.Configuration.Services;

public static class ConfigureEventBusExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostBuilder UseCustomEventBus(this IHostBuilder builder)
    {
        builder.UseNServiceBus(context =>
        {
            var endpointConfiguration = new EndpointConfiguration("TripService");
                
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>()
                .ConnectionString(context.Configuration.GetConnectionString("RabbitMq"))
                .UseConventionalRoutingTopology();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.EnableCallbacks();
            endpointConfiguration.EnableOutbox();
            endpointConfiguration.MakeInstanceUniquelyAddressable(Environment.MachineName);
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.SendFailedMessagesTo(errorQueue: "error");

            //Routing
            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(SubmitTrip).Assembly, "NServiceBus.Saga.Demo.Contracts.Trips", "TripService");
            routing.RouteToEndpoint(typeof(BookFlightRequest).Assembly, "NServiceBus.Saga.Demo.Contracts.Flights", "TransportationService");
            routing.RouteToEndpoint(typeof(BookHotelRequest).Assembly, "NServiceBus.Saga.Demo.Contracts.Hotels", "HotelService");

            //Monitoring and auditing
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.AuditSagaStateChanges(serviceControlQueue: "audit");

            endpointConfiguration.EnableMetrics().SendMetricDataToServiceControl(
                serviceControlMetricsAddress: "Particular.Monitoring",
                interval: TimeSpan.FromSeconds(2)
            );
            endpointConfiguration.ConnectToServicePlatform(context.Configuration.GetSection("NServiceBus").Get<ServicePlatformConnectionConfiguration>());


            return endpointConfiguration;
        });
        return builder;
    }
}