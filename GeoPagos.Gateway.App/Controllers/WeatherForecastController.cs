using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace GeoPagos.Gateway.App.Controllers
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
        private readonly Tracer _tracer;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, TracerProvider tracerProvider)
        {
            _logger = logger;
            _tracer = tracerProvider.GetTracer("WeatherForecastController");
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            PropagateTracing("WeatherForecastController", "GetWeatherForecast");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        private void PropagateTracing(string controller, string method)
        {
            using var span = _tracer.StartActiveSpan($"{controller}.{method}");

            span.SetAttribute("controller", controller);
            span.SetAttribute("method", method);
            span.AddEvent("Start of method");

            // Simular una operación
            System.Threading.Thread.Sleep(100);

            span.AddEvent("End of method");

            // Finalizar el span
            span.End();
        }
    }


}
