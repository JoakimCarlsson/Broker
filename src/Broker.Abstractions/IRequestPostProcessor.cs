namespace Broker.Abstractions;

public interface IRequestPostProcessor<in TRequest, in TResponse>
{
    Task ProcessAsync(TRequest request, TResponse response, CancellationToken cancellationToken = default);
}
