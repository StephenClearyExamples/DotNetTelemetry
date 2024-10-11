using OpenTelemetry;
using WorkerService;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureOpenTelemetry();
builder.Services.AddOpenTelemetry().UseOtlpExporter();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
