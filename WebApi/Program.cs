using Nito.Logging;
using OpenTelemetry;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureOpenTelemetry();

// Export to local OTLP.
builder.Services.AddOpenTelemetry().UseOtlpExporter();

builder.Services.AddExceptionLoggingScopes();

builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<QueueService>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseAuthorization();
app.MapControllers();
app.Run();
