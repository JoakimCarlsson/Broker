namespace Broker.Example.Features.Nothing.Get;

internal sealed class GetNothingPreProcessor : IRequestPreProcessor<GetNothingCommand>
{
    private readonly ILogger<GetNothingPreProcessor> _logger;

    public GetNothingPreProcessor(ILogger<GetNothingPreProcessor> logger)
    {
        _logger = logger;
    }
    
    public Task ProcessAsync(GetNothingCommand request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing request: {Request}", request);
        return Task.CompletedTask;
    }
}