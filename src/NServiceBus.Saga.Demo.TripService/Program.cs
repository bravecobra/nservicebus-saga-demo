using NServiceBus.Saga.Demo.TripService.Configuration.Services;
using Serilog;

namespace NServiceBus.Saga.Demo.TripService;

public class Program
{
    static void Main(string[] args)
    {
        //Basic console logging as soon as possible
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting up");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }

    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseCustomEventBus()
            .UseCustomLogging()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}