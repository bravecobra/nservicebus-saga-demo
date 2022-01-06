using NServiceBus.Common;
using NServiceBus.Saga.Demo.Contracts.Flights;
using NServiceBus.Saga.Demo.Contracts.Hotels;
using NServiceBus.Saga.Demo.Contracts.Trips;

namespace NServiceBus.Saga.Demo.TripService.Configuration.Services
{
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
                    .ConnectionString("amqp://localhost")
                    .UseConventionalRoutingTopology();
                endpointConfiguration.UsePersistence<InMemoryPersistence>();
                endpointConfiguration.EnableInstallers();
                endpointConfiguration.EnableCallbacks();
                endpointConfiguration.EnableOutbox();
                endpointConfiguration.MakeInstanceUniquelyAddressable(Environment.MachineName);
                endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
                endpointConfiguration.SendFailedMessagesTo(errorQueue: "error");

                var routing = transport.Routing();
                routing.RouteToEndpoint(typeof(SubmitTrip), "TripService");
                routing.RouteToEndpoint(typeof(TripSubmissionResponse), "TripService");
                routing.RouteToEndpoint(typeof(TripRegistrationRequest), "TripService");
                routing.RouteToEndpoint(typeof(BookFlightRequest), "BookFlight");
                routing.RouteToEndpoint(typeof(BookHotelRequest), "BookHotel");

                return endpointConfiguration;
            });
            QueueCreationUtils.CreateQueuesForEndpoint(
                uri: "amqp://guest:guest@localhost:5672",
                endpointName: "BookFlight",
                durableMessages: true,
                createExchanges: true);

            QueueCreationUtils.CreateQueuesForEndpoint(
                uri: "amqp://guest:guest@localhost:5672",
                endpointName: "BookHotel",
                durableMessages: true,
                createExchanges: true);
            return builder;
        }
    }
}
