using OpenTelemetry;
using WorkerService;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureOpenTelemetry()
	.Services.AddOpenTelemetry().WithTracing(
		tracing => tracing.AddSource(Worker.ActivitySourceName)
	);
builder.Services.AddOpenTelemetry().UseOtlpExporter();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
