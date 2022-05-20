using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
        private readonly ILogger<ScheduledEchoBackgroundService> _logger;
        private readonly string _slackBaseUrl;
        private readonly string _slackChannel;

        public ScheduledEchoBackgroundService(IConfiguration configuration, ILogger<ScheduledEchoBackgroundService> logger)
        {
            _logger = logger;
            _urls = String.IsNullOrEmpty(MicroService.MicroServicesURL) ? new string[] { } : MicroService.MicroServicesURL.Replace(" ", string.Empty).Split(',');
            if (!string.IsNullOrEmpty(WebHooks.SlackChannelHook))
            {
                (_slackBaseUrl, _slackChannel) = GeneralHelper.SplitSlackHook(WebHooks.SlackChannelHook);
            }

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
            var notify = new NotificationServices(_slackBaseUrl, _slackChannel);
            foreach (var url in _urls)
            {
                try
                {
                    using var channel = GrpcChannel.ForAddress(url);
                    var client = new EchoRPCService.EchoRPCServiceClient(channel);
                    var reply = await client.SendEchoAsync(new Empty());
                    _logger.LogInformation($"Echo Response: {reply.StatusCode} - {reply.Message} -- {DateTime.Now}");
                }
                catch (RpcException ex) when (ex.StatusCode != StatusCode.OK)
                {
                    //Send Server Down Alert
                    await notify.SendSlackNotificationAsync("Echo Test server can not echo " + url);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
