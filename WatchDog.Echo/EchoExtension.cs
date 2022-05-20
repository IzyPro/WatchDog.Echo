using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using WatchDog.Echo.src.Models;
using WatchDog.Echo.src.Services;

namespace WatchDog.Echo
{
    public static class EchoExtension
    {
        public static IServiceCollection AddWatchDogEchoServices(this IServiceCollection services, [Optional] Action<EchoSettings> configureOptions)
        {
            var options = new EchoSettings();
            if(configureOptions != null)
                configureOptions(options);
          
            if(options != null)
            {
                EchoInterval.EchoIntervalInMinutes = options.EchoIntervalInMinutes;
                MicroService.MicroServicesURL = options.HostURLs;
                WebHooks.SlackChannelHook = options.SlackChannelAddress;
            }

            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });
            if(options != null)
            {
                services.AddHostedService<ScheduledEchoBackgroundService>();
            }
            return services;
        }
        public static IApplicationBuilder UseWatchDogEcho(this IApplicationBuilder app)
        {
            app.UseRouting();
            return app.UseEndpoints(endpoints =>
            {
                //Registering an endpoint for non server application (worker service)
                endpoints.MapGet("echo", async context =>
                {
                    context.Response.ContentType = "text/html";
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await context.Response.WriteAsync("Echo is listening");
                });
                endpoints.MapGrpcService<src.Services.EchoServices>();
            });
        }
    }
}
