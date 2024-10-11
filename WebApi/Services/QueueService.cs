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
		_channel.QueueDeclare(queue: "weather_control_request",
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

	public void PublishWeatherControlRequest(DateOnly date, int temperature)
	{
		_logger.LogInformation("Publishing weather control request: {date}, {temperature}", date, temperature);

		var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { date, temperature }));
		_channel.BasicPublish(exchange: "",
								routingKey: "weather_control_request",
								basicProperties: null,
								body: body);
	}
}
