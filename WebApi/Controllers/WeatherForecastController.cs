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
	private readonly DatabaseService _databaseService;

	public WeatherForecastController(ILogger<WeatherForecastController> logger, WeatherForecastService forecastService, QueueService queueService, DatabaseService databaseService)
	{
		_logger = logger;
		_service = forecastService;
		_queueService = queueService;
		_databaseService = databaseService;
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
	public async Task Post(DateOnly? date, int? temperature)
	{
		if (date == null || temperature == null)
			throw new ArgumentException("Date and temperature are required.");

		var workId = await _databaseService.InsertRequestedWorkAsync();
		_queueService.PublishWeatherControlRequest(new(date.Value, temperature.Value, workId));
	}
}
