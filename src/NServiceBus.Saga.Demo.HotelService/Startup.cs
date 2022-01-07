using NServiceBus.Saga.Demo.HotelService.Configuration.Services;

namespace NServiceBus.Saga.Demo.HotelService;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCompositionRoot();
        services.AddRouting();

        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UsePathBase(Configuration["BasePath"]);
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseStaticFiles();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}