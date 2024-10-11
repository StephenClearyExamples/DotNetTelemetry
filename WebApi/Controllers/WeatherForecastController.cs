using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
	private readonly ILogger<WeatherForecastController> _logger;
	private readonly WeatherForecastService _service;
	private readonly QueueService _queueService;

	public WeatherForecastController(ILogger<WeatherForecastController> logger, WeatherForecastService forecastService, QueueService queueService)
	{
		_logger = logger;
		_service = forecastService;
		_queueService = queueService;
	}

	[HttpGet]
	public IEnumerable<WeatherForecast> Get()
	{
		var today = DateOnly.FromDateTime(DateTime.Now);
		return Enumerable.Range(1, 5)
			.Select(index => _service.GetWeatherForecast(today.AddDays(index)))
			.ToArray();
	}

	[HttpPost]
	public void Post(DateOnly? date, int? temperature)
	{
		if (date == null || temperature == null)
			throw new ArgumentException("Date and temperature are required.");

		_queueService.PublishWeatherControlRequest(date.Value, temperature.Value);
	}
}
