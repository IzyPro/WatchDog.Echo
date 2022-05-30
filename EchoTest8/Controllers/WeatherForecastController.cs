using Microsoft.AspNetCore.Mvc;
using WatchDog.Echo.src.Events;

namespace EchoTest8.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
         };


        private readonly ILogger<WeatherForecastController> _logger;
        private EchoEventSubscriber _subscriber;   

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            EchoEventSubscriber _subscriber = new EchoEventSubscriber();
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            _subscriber.Subscribe(e_OnEventFailed);
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        static void e_OnEventFailed(object sender, EchoEventsArgs e)
        {
            Console.WriteLine("The host {0} couldnt reach {1}.", e.FromHost, e.ToHost);
        }
    }
}