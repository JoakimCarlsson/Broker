namespace Broker.Example.Features.Nothing.Get;

internal sealed class GetNothingPostProcessor : IRequestPostProcessor<GetNothingCommand>
{
    private readonly ILogger<GetNothingPostProcessor> _logger;

    public GetNothingPostProcessor(ILogger<GetNothingPostProcessor> logger)
    {
        _logger = logger;
    }

    public Task ProcessAsync(GetNothingCommand request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing request: {Request}", request);
        return Task.CompletedTask;
    }
}