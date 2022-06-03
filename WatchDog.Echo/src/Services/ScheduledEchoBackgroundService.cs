using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WatchDog.Echo.src.Events;
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
        private readonly string _clientHost;
        private NotificationServices notify;

        public ScheduledEchoBackgroundService(ILogger<ScheduledEchoBackgroundService> logger)
        {
            _logger = logger;
            alertFrequency = new Dictionary<string, DateTime>();
            _urls = string.IsNullOrEmpty(MicroService.MicroServicesURL) ? new string[] { } : MicroService.MicroServicesURL.Replace(" ", string.Empty).Split(',');
            _webhooks = string.IsNullOrEmpty(WebHooks.WebhookURLs) ? new string[] { } : WebHooks.WebhookURLs.Replace(" ", string.Empty).Split(',');
            _toEmailAddresses = string.IsNullOrEmpty(MailAlerts.ToEmailAddress) ? new string[] { } : MailAlerts.ToEmailAddress.Replace(" ", string.Empty).Split(',');
            _mailSettings = MailConfiguration.MailConfigurations;
            _clientHost = MicroService.MicroServiceClientHost;
            notify = new NotificationServices();
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
                if (Protocol.ProtocolType == Enums.ProtocolEnum.gRPC)
                    await EchoGRPCCallAsync();
                else
                    await EchoRESTCallAsync();
                isProcessing = false;
            }
        }

        private async Task EchoGRPCCallAsync()
        {
            //Initialize Notification Service once
            foreach (var url in _urls)
            {
                try
                {
                    using var channel = GrpcChannel.ForAddress(url);
                    var client = new EchoRPCService.EchoRPCServiceClient(channel);
                    var reply = await client.SendEchoAsync(new EchoRequest { IsReverb = true });
                    _logger.LogInformation($"Echo Response: {reply.StatusCode} - {reply.Message} -- {DateTime.Now} -- {System.Reflection.Assembly.GetEntryAssembly().GetName().Name}");
                    //Recall Reverb If True
                    if (reply.IsReverb)
                    {
                        channel.Dispose();
                        //Flip Case
                        await ReverbEchoAsync(url, _clientHost);
                    }
                }
                catch (RpcException ex) when (ex.StatusCode != StatusCode.OK)
                {
                    await CheckAndSendAlert(url, ex.StatusCode.ToString());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private async Task EchoRESTCallAsync()
        {
            var restService = new EchoRESTService();
            foreach (var url in _urls)
            {
                try
                {
                    var response = await restService.SendRESTEcho(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        await CheckAndSendAlert(url, response.StatusCode.ToString());
                    }
                }
                catch (HttpRequestException ex) {
                    await CheckAndSendAlert(url, ex.Message);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private async Task CheckAndSendAlert(string url, string message)
        {
            if (!alertFrequency.ContainsKey(url))
            {
                alertFrequency.Add(url, DateTime.Now);
            }
            else
            {
                var difference = DateTime.Now.Subtract(alertFrequency[url]);
                if (difference.TotalMinutes < EchoInterval.FailedEchoAlertIntervalInMinutes)
                    return;
                else
                    alertFrequency[url] = DateTime.Now;
            }
            //Send Server Down Alert
            EchoEventPublisher.Instance.PublishEchoFailedEvent(_clientHost, url);
            if (_toEmailAddresses.Length > 0 && _mailSettings != null)
            {
                await notify.SendEmailNotificationAsync(url, message, _toEmailAddresses, _mailSettings);
            }
            if (!string.IsNullOrEmpty(WebHooks.CustomAlertWebhookURL))
                notify.SendCustomAlertWebhookNotificationAsync(message, url, DateTime.Now);
            foreach (var webhook in _webhooks)
            {
                await HandleNotification(url, message, false, webhook);
            }
        }


        private async Task ReverbEchoAsync(string clientHost, string serverHost)
        {
            //Try Catch Notify Once
            try
            {
                //Reverb
                using var reverbChannel = GrpcChannel.ForAddress(serverHost);
                var echoClient = new EchoRPCService.EchoRPCServiceClient(reverbChannel);
                echoClient.WithHost(clientHost);
                var reverbReply = await echoClient.ReverbEchoAsync(new Empty());
            }
            catch (RpcException ex) when (ex.StatusCode != StatusCode.OK)
            {
                await CheckAndSendAlert(serverHost, ex.StatusCode.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task HandleNotification(string toUrl, string ex, bool isReverb, string webhook)
        {
            var projectName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            var action = isReverb ? "reverb" : "echo";
            var (_webhookBaseUrl, _webhookEndpoint) = GeneralHelper.SplitWebhook(webhook);
            var message = $"ALERT!!!\n{toUrl} failed to respond to {action} from {_clientHost} ({projectName}).\nResponse: {ex}\nHappened At: {DateTime.Now.ToString("dd/MM/yyyy hh:mm tt")}";
            await notify.SendWebhookNotificationAsync(message, _webhookBaseUrl, _webhookEndpoint);
        }
    }
}
