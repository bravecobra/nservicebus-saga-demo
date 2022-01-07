using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NServiceBus.Persistence.Sql;
using NServiceBus.Saga.Demo.Contracts.Flights;
using NServiceBus.Saga.Demo.Contracts.Hotels;
using NServiceBus.Saga.Demo.Contracts.Trips;
using NServiceBus.Saga.Demo.TripService.Persistence;

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

            //SagaData Persistence
            //endpointConfiguration.UsePersistence<InMemoryPersistence>();
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();

            persistence.ConnectionBuilder(() => new SqlConnection(context.Configuration.GetConnectionString("trip-database")));
            endpointConfiguration.RegisterComponents(c =>
            {
                 c.ConfigureComponent(b =>
                 {
                     var session = b.Build<ISqlStorageSession>();
                     var dbContext = new TripDbContext(new DbContextOptionsBuilder<TripDbContext>()
                         .UseSqlServer(session.Connection)
                         .Options);
                     //
                     //Use the same underlying ADO.NET transaction
                     dbContext.Database.UseTransaction(session.Transaction);
            
                     //Ensure context is flushed before the transaction is committed
                     session.OnSaveChanges(s => dbContext.SaveChangesAsync());
            
                     return dbContext;
                 }, DependencyLifecycle.InstancePerUnitOfWork);
            });
            var subscriptions = persistence.SubscriptionSettings();
            subscriptions.CacheFor(TimeSpan.FromMinutes(1));

            return endpointConfiguration;
        });
        return builder;
    }
}