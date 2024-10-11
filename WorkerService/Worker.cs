using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Polly.Retry;
using Polly;
using System.Diagnostics;
using Nito.Logging;

namespace WorkerService;

public class Worker : BackgroundService
{
	public const string ActivitySourceName = "WorkerService.Worker";

	private static readonly ActivitySource _activitySource = new(ActivitySourceName);
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
		// Temporary hack until .net aspire 9 lets us wait for dependencies in the orchestrator.
		ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
			.AddRetry(new RetryStrategyOptions
			{
				MaxRetryAttempts = int.MaxValue,
			})
			.AddTimeout(TimeSpan.FromMinutes(1))
			.Build();
		var (connection, channel) = await pipeline.ExecuteAsync(static async token =>
		{
			var factory = new ConnectionFactory { HostName = "localhost" };
			var connection = factory.CreateConnection();
			var channel = connection.CreateModel();

			channel.QueueDeclare(queue: Messages.Constants.WeatherControlRequestQueue,
								 durable: true,
								 exclusive: false,
								 autoDelete: false,
								 arguments: null);
			return (connection, channel);
		}, stoppingToken);
		using var _ = connection;
		using var __ = channel;

		var consumer = new EventingBasicConsumer(channel);
		consumer.Received += (model, ea) =>
		{
			// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md#span-name
			using var activity = _activitySource.StartActivity($"{Messages.Constants.WeatherControlRequestQueue} receive");
			
			// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md#messaging-attributes
			// https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/rabbitmq.md
			activity?.SetTag("server.address", "localhost");
			activity?.SetTag("messaging.destination.name", Messages.Constants.WeatherControlRequestQueue);

			using var _ = _logger.BeginDataScope(("QueueServer", "localhost"), ("QueueName", Messages.Constants.WeatherControlRequestQueue));

			var body = ea.Body.ToArray();
			var message = Encoding.UTF8.GetString(body);
			var json = JsonSerializer.Deserialize<Messages.WeatherForecastRequest>(message) ?? throw new Exception("Bad message");
			_logger.LogInformation("Received weather control request for {date} {temperature}.", json.Date, json.Temperature);
		};
		channel.BasicConsume(queue: Messages.Constants.WeatherControlRequestQueue,
							 autoAck: true,
							 consumer: consumer);

		_logger.LogInformation("Waiting for messages.");

		await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
