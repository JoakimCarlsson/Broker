namespace Broker.Example.Features.Weather.Get;

internal sealed record GetWeatherForecastCommand : IRequest<GetWeatherForecastResponse[]>;

internal sealed record GetWeatherForecastResponse(
    DateOnly Date,
    int TemperatureC,
    string? Summary
)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal sealed class GetWeatherHandler : IHandler<GetWeatherForecastCommand, GetWeatherForecastResponse[]>
{
    private readonly string[] _summaries =
    [
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    ];
    
    public Task<GetWeatherForecastResponse[]> HandleAsync(GetWeatherForecastCommand request, CancellationToken cancellationToken = default)
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new GetWeatherForecastResponse
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    _summaries[Random.Shared.Next(_summaries.Length)]
                ))
            .ToArray();
        
        return Task.FromResult(forecast);
    }
}