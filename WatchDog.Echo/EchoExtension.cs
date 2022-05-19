using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WatchDog.Echo.src.Models;
using WatchDog.Echo.src.Services;

namespace WatchDog.Echo
{
    public static class EchoExtension
    {
        public static IServiceCollection AddEchoServices(this IServiceCollection services, Action<EchoSettings> configureOptions)
        {
            var options = new EchoSettings();
            if(configureOptions != null)
                configureOptions(options);
            EchoInterval.EchoIntervalInMinutes = options.EchoIntervalInMinutes;
            MicroService.MicroServicesURL = options.HostURLs;
            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });
            services.AddHostedService<ScheduledEchoBackgroundService>();
            return services;
        }
        public static IApplicationBuilder UseEcho(this IApplicationBuilder app)
        {
            app.UseRouting();
            return app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<src.Services.EchoServices>();
            });
        }
    }
}
