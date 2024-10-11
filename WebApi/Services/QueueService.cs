using Messages;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace WebApi.Services;

public sealed class QueueService : IDisposable
{
	private readonly ConnectionFactory _connectionFactory;
	private readonly IConnection _connection;
	private readonly IModel _channel;
	private readonly ILogger<QueueService> _logger;

	public QueueService(ILogger<QueueService> logger)
	{
		_logger = logger;
		_connectionFactory = new ConnectionFactory() { HostName = "localhost" };
		_connection = _connectionFactory.CreateConnection();
		_channel = _connection.CreateModel();
		_channel.QueueDeclare(queue: Messages.Constants.WeatherControlRequestQueue,
								durable: true,
								exclusive: false,
								autoDelete: false,
								arguments: null);
	}

	void IDisposable.Dispose()
	{
		_channel.Dispose();
		_connection.Dispose();
	}

	public void PublishWeatherControlRequest(WeatherForecastRequest message)
	{
		_logger.LogInformation("Publishing weather control request: {date}, {temperature}", message.Date, message.Temperature);

		var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
		_channel.BasicPublish(exchange: "",
								routingKey: Messages.Constants.WeatherControlRequestQueue,
								basicProperties: null,
								body: body);
	}
}
