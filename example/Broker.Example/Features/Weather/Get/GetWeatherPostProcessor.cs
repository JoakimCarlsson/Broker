namespace Broker.Example.Features.Weather.Get;

internal sealed class GetWeatherPostProcessor : IRequestPostProcessor<GetWeatherForecastCommand, GetWeatherForecastResponse[]>
{
    private readonly ILogger<GetWeatherPostProcessor> _logger;

    public GetWeatherPostProcessor(ILogger<GetWeatherPostProcessor> logger)
    {
        _logger = logger;
    }
    
    public Task ProcessAsync(
        GetWeatherForecastCommand request,
        GetWeatherForecastResponse[] response,
        CancellationToken cancellationToken = default
        )
    {
        _logger.LogInformation("Processed request: {Request}, response: {Response}", request, response);
        return Task.CompletedTask;
    }
}