namespace Broker.Example.Features.Nothing.Get;

internal sealed record GetNothingCommand : IRequest;

internal sealed class GetNothingHandler : IHandler<GetNothingCommand>
{
    private readonly ILogger<GetNothingHandler> _logger;

    public GetNothingHandler(ILogger<GetNothingHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(
        GetNothingCommand request,
        CancellationToken cancellationToken = default
        )
    {
        _logger.LogInformation("I'm doing nothing.");
        return Task.CompletedTask;
    }
}