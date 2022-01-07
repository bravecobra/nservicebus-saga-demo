using NServiceBus.Saga.Demo.TripService.Configuration.Services;

namespace NServiceBus.Saga.Demo.TripService;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context,collection) =>
            {
                collection.AddCustomPersistence(context.Configuration);
            })
            .UseCustomEventBus()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}