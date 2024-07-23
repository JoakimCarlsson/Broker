namespace Broker.Abstractions;

public interface ISender
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest;
}