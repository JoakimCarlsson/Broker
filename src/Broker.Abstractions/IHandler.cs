namespace Broker.Abstractions;

public interface IHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IHandler<in TRequest> where TRequest : IRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}