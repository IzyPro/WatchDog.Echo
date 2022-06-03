using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
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

            Protocol.ProtocolType = options.Protocol;
            if (Protocol.ProtocolType == src.Enums.ProtocolEnum.gRPC)
                services.AddGrpc();

            if (string.IsNullOrEmpty(options.ClientHost))
            {
                throw new ArgumentNullException(nameof(options.ClientHost));
            }
            else
            {
                MicroService.MicroServiceClientHost = options.ClientHost;
            }

            if (options != null && options.EchoTargetURLs?.Length > 0)
            {
                EchoInterval.EchoIntervalInMinutes = options.EchoIntervalInMinutes;
                EchoInterval.FailedEchoAlertIntervalInMinutes = options.FailedEchoAlertIntervalInMinutes;
                MailAlerts.ToEmailAddress = options.EmailAddresses;
                MicroService.MicroServicesURL = options.EchoTargetURLs;
                WebHooks.WebhookURLs = options.WebhookURLs;
                WebHooks.CustomAlertWebhookURL = options.CustomAlertWebhookURL;

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
            }
            services.AddSingleton<IStartupFilter, EchoStartupFilter>();
            return services;
        }
    }
}
