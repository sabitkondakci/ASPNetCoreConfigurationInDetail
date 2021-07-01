using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Configure.Controllers
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
        private readonly IOptions<CustomOptions> _options;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,IOptions<CustomOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            Console.WriteLine($"Url:{_options.Value.Url}\nPublicKey: {_options.Value.PublicKey}\nUser-Secrets-SessionKey:{_options.Value.SessionKey}\nUser-Secrets-PreMasterKey:{_options.Value.PreMasterKey}\nUser-Secrets-PrivateKey:{_options.Value.PrivateKey}");
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
