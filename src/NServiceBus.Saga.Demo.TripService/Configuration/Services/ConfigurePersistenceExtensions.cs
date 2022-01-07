using Microsoft.EntityFrameworkCore;
using NServiceBus.Saga.Demo.TripService.Infrastructure;
using NServiceBus.Saga.Demo.TripService.Persistence;

namespace NServiceBus.Saga.Demo.TripService.Configuration.Services
{
    internal static class ConfigurePersistenceExtensions
    {
        public static void AddCustomPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionstring = configuration.GetConnectionString("trip-database");
            if (Common.Container.IsRunningInContainer)
            {
                connectionstring = connectionstring.Replace("localhost", "mssql");
            }

            services.AddDbContext<TripDbContext>(builder =>
                builder.UseSqlServer(connectionstring));

            // So we don't need to use ef migrations for this sample.
            // Likely if you are going to deploy to a production environment, you want a better DB deploy strategy.
            services.AddHostedService<EfDbCreatedHostedService>();
        }
    }
}
