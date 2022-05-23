using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WatchDog.Echo.src.Models;
using WatchDog.Echo.src.Utilities;

namespace WatchDog.Echo.src.Services
{
    internal class ScheduledEchoBackgroundService : BackgroundService
    {
        private bool isProcessing;
        private readonly string[] _urls;
        private readonly string[] _webhooks;
        private readonly ILogger<ScheduledEchoBackgroundService> _logger;
        private Dictionary<string, DateTime> alertFrequency;
        private readonly string[] _toEmailAddresses;
        private readonly MailSettings _mailSettings;

        public ScheduledEchoBackgroundService(ILogger<ScheduledEchoBackgroundService> logger)
        {
            _logger = logger;
            alertFrequency = new Dictionary<string, DateTime>();
            _urls = string.IsNullOrEmpty(MicroService.MicroServicesURL) ? new string[] { } : MicroService.MicroServicesURL.Replace(" ", string.Empty).Split(',');
            _webhooks = string.IsNullOrEmpty(WebHooks.WebhookURLs) ? new string[] { } : WebHooks.WebhookURLs.Replace(" ", string.Empty).Split(',');
            _toEmailAddresses = string.IsNullOrEmpty(MailAlerts.ToEmailAddress) ? new string[] { } : MailAlerts.ToEmailAddress.Replace(" ", string.Empty).Split(',');
            _mailSettings = MailConfiguration.MailConfigurations;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!isProcessing)
                {
                    isProcessing = true;
                }
                else
                {
                    return;
                }

                TimeSpan minute = TimeSpan.FromMinutes((double)EchoInterval.EchoIntervalInMinutes);
                var start = DateTime.UtcNow;
                while (true)
                {
                    var remaining = (minute - (DateTime.UtcNow - start)).TotalMilliseconds;
                    if (remaining <= 0)
                        break;
                    if (remaining > Int16.MaxValue)
                        remaining = Int16.MaxValue;
                    await Task.Delay(TimeSpan.FromMilliseconds(remaining));
                }
                await EchoCallAsync();
                isProcessing = false;
            }
        }

        private async Task EchoCallAsync()
        {
            //Initialize Notification Service once
            var notify = new NotificationServices();
            foreach (var url in _urls)
            {
                try
                {
                    using var channel = GrpcChannel.ForAddress(url);
                    var client = new EchoRPCService.EchoRPCServiceClient(channel);
                    var reply = await client.SendEchoAsync(new EchoRequest { IsReverb = true});
                    _logger.LogInformation($"Echo Response: {reply.StatusCode} - {reply.Message} -- {DateTime.Now} -- {Constants.CurrentHost} -- {System.Reflection.Assembly.GetEntryAssembly().GetName().Name}");
                    //Recall Reverb If True
                    if (reply.IsReverb)
                    {
                        await ReverbEchoAsync(url, reply.CallerHost);
                    }
                }
                catch (RpcException ex) when (ex.StatusCode != StatusCode.OK)
                {
                    if (!alertFrequency.ContainsKey(url))
                    {
                        alertFrequency.Add(url, DateTime.Now);
                    }
                    else
                    {
                        var difference = DateTime.Now.Subtract(alertFrequency[url]);
                        if (difference.TotalMinutes < EchoInterval.FailedEchoAlertIntervalInMinutes)
                            continue;
                        else
                            alertFrequency[url] = DateTime.Now;
                    }
                    //Send Server Down Alert
                    foreach (var webhook in _webhooks)
                    {
                        var fromHost = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                        await HandleNotification(notify, fromHost, url, ex);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        private async Task ReverbEchoAsync(string url, string callerHost)
        {
            var notify = new NotificationServices();
            //Try Catch Notify Once
            try
            {
                //Reverb

                using var reverbChannel = GrpcChannel.ForAddress(callerHost);
                var echoClient = new EchoRPCService.EchoRPCServiceClient(reverbChannel);
                var reverbReply = await echoClient.ReverbEchoAsync(new Empty());

            }
            catch (RpcException ex) when (ex.StatusCode != StatusCode.OK)
            {
                await HandleNotification(notify, url, callerHost, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task HandleNotification(NotificationServices notify, string fromUrl, string toUrl, RpcException ex)
        {
            var (_webhookBaseUrl, _webhookEndpoint) = GeneralHelper.SplitWebhook(WebHooks.WebhookURLs);
            var message = $"ALERT!!!\nEcho Test server ({fromUrl}) could not echo {toUrl}.\nResponse: {ex.StatusCode}\nHappened At: {DateTime.Now.ToString("dd/MM/yyyy hh:mm tt")}";
            await notify.SendWebhookNotificationAsync(message, _webhookBaseUrl, _webhookEndpoint);
            if (_toEmailAddresses.Length > 0 && _mailSettings != null)
            {
                await notify.SendEmailNotificationAsync(message, _toEmailAddresses, _mailSettings);
            }
        }
    }
}
