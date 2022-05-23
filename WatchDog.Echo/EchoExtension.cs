using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using WatchDog.Echo.src;
using WatchDog.Echo.src.Exceptions;
using WatchDog.Echo.src.Models;
using WatchDog.Echo.src.Services;
using WatchDog.Echo.src.Utilities;

namespace WatchDog.Echo
{
    public static class EchoExtension
    {
        public static IServiceCollection AddWatchDogEchoServices(this IServiceCollection services, [Optional] Action<EchoSettings> configureOptions)
        {
            var options = new EchoSettings();
            if (configureOptions != null)
                configureOptions(options);

            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });

            if (options != null)
            {
                EchoInterval.EchoIntervalInMinutes = options.EchoIntervalInMinutes;
                EchoInterval.FailedEchoAlertIntervalInMinutes = options.FailedEchoAlertIntervalInMinutes;
                MailAlerts.ToEmailAddress = options.EmailAddresses;
                MicroService.MicroServicesURL = options.HostURLs;
                WebHooks.WebhookURLs = options.WebhookURLs;

                //Handle cases where mail option is passed and not Email Address
                if(options.MailConfig != null && string.IsNullOrEmpty(options.EmailAddresses))
                {
                    throw new WatchDogEchoMailSettingsException("Empty Email Address field");
                }

                if (!string.IsNullOrEmpty(options.EmailAddresses))
                {
                    if(options.MailConfig == null)
                    {
                        //Throw null mail configuration exception
                        throw new WatchDogEchoMailSettingsException("Null MailSettings Configuration");
                    }
                    else
                    {
                        var (hasEmptyField, field) = options.MailConfig.IsAnyNullOrEmpty();
                        if (hasEmptyField)
                        {
                            //Throw empty mail settings
                            throw new WatchDogEchoMailSettingsException($"Mail Settings Property '{field}' has an empty field");
                        }
                    }
                }

                

                MailConfiguration.MailConfigurations = options.MailConfig;

                services.AddHostedService<ScheduledEchoBackgroundService>();
                services.AddSingleton<IStartupFilter, EchoStartupFilter>();
            }
            return services;
        }
        //public static IApplicationBuilder UseWatchDogEcho(this IApplicationBuilder app)
        //{
        //    app.UseRouting();
        //    return app.UseEndpoints(endpoints =>
        //    {
        //        //Registering an endpoint for non server application (worker service)
        //        endpoints.MapGet("echo", async context =>
        //        {
        //            context.Response.ContentType = "text/html";
        //            context.Response.StatusCode = (int)HttpStatusCode.OK;
        //            await context.Response.WriteAsync("Echo is listening");
        //        });
        //        endpoints.MapGrpcService<EchoServices>();
        //    });
        //}
    }
}
