namespace Messages;

public sealed record class WeatherForecastRequest(DateOnly Date, int Temperature, int WorkId);
