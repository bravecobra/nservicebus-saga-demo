using NServiceBus.Saga.Demo.Contracts.Hotels;

namespace NServiceBus.Saga.Demo.HotelService.Configuration.Services
{
    public static class ConfigureEventBusExtensions
    {
        public static IHostBuilder UseCustomEventBus(this IHostBuilder builder)
        {
            builder.UseNServiceBus(context =>
            {
                var endpointConfiguration = new EndpointConfiguration("BookHotel");

                var transport = endpointConfiguration.UseTransport<RabbitMQTransport>()
                    .ConnectionString("amqp://localhost")
                    .UseConventionalRoutingTopology();
                endpointConfiguration.UsePersistence<InMemoryPersistence>();
                endpointConfiguration.EnableInstallers();
                // endpointConfiguration.EnableCallbacks();
                endpointConfiguration.EnableOutbox();
                endpointConfiguration.MakeInstanceUniquelyAddressable(Environment.MachineName);
                endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
                endpointConfiguration.SendFailedMessagesTo(errorQueue: "error");

                var routing = transport.Routing();
                routing.RouteToEndpoint(typeof(HotelBooked), "TripService");
                return endpointConfiguration;
            });
            // builder.UseNServiceBus(context =>
            // {
            //     var endpointConfiguration = new EndpointConfiguration("BookFlight");
            //     return endpointConfiguration;
            // });
            // QueueCreationUtils.CreateQueuesForEndpoint(
            //     uri: "amqp://guest:guest@localhost:5672",
            //     endpointName: "BookFlight",
            //     durableMessages: true,
            //     createExchanges: true);
            return builder;
        }
    }
}
