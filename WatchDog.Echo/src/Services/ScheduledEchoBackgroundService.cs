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

namespace WatchDog.Echo.src.Services
{
    internal class ScheduledEchoBackgroundService : BackgroundService
    {
        private bool isProcessing;
        private readonly string[] _urls;
        private readonly ILogger<ScheduledEchoBackgroundService> _logger;

        public ScheduledEchoBackgroundService(IConfiguration configuration, ILogger<ScheduledEchoBackgroundService> logger)
        {
            _logger = logger;
            _urls = String.IsNullOrEmpty(MicroService.MicroServicesURL) ? new string[] { } : MicroService.MicroServicesURL.Replace(" ", string.Empty).Split(',');
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

                TimeSpan minute = TimeSpan.FromMinutes(EchoInterval.EchoIntervalInMinutes);
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
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
