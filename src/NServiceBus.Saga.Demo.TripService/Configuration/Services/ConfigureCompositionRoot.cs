﻿namespace NServiceBus.Saga.Demo.TripService.Configuration.Services
{
    public static class ConfigureCompositionRoot
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddCompositionRoot(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddOptions();
            return services;
        }
    }
}
