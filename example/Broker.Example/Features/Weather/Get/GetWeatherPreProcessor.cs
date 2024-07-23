namespace Broker.Example.Features.Weather.Get;

internal sealed class GetWeatherPreProcessor : IRequestPreProcessor<GetWeatherForecastCommand>
{
    private readonly ILogger<GetWeatherPreProcessor> _logger;

    public GetWeatherPreProcessor(ILogger<GetWeatherPreProcessor> logger)
    {
        _logger = logger;
    }
    
    public Task ProcessAsync(GetWeatherForecastCommand request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing request: {Request}", request);
        return Task.CompletedTask;
    }
}