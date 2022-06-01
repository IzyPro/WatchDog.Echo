using WatchDog.Echo.src.Events;

namespace EchoTest8.Services
{
    public class WorkerBackgroundService : BackgroundService
    {
        private readonly ILogger<WorkerBackgroundService> _logger;

        public WorkerBackgroundService(ILogger<WorkerBackgroundService> logger)
        {
            _logger = logger;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EchoEventPublisher.Instance.OnEchoFailedEvent += e_OnEventFailed;
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

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
