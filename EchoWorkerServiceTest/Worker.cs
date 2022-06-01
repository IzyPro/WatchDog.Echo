using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WatchDog.Echo.src.Events;

namespace EchoWorkerServiceTest
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //var _subscriber = new EchoEventPublisher();
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                EchoEventPublisher.EchoFailedEvent += e_OnEventFailed;
                await Task.Delay(1000, stoppingToken);
            }
        }

        static void e_OnEventFailed(object sender, EchoEventsArgs e)
        {
            //Handle Echo Failed Event
            Console.WriteLine("The host {0} couldnt reach {1}.", e.FromHost, e.ToHost);
        }
    }
}
