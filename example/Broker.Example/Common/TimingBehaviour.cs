namespace Broker.Example.Common;

internal sealed class TimingBehaviour<TRequest, TResponse> : IRequestPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken = default
        )
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();
        Console.WriteLine($"Request {typeof(TRequest).Name} took {stopwatch.ElapsedMilliseconds}ms");
        return response;
    }
}