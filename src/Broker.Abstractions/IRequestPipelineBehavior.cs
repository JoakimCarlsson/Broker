namespace Broker.Abstractions;

public interface IRequestPipelineBehavior<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default
    );
}

public interface IRequestPipelineBehavior<TRequest>
{
    Task HandleAsync(
        TRequest request,
        RequestHandlerDelegate next,
        CancellationToken cancellationToken = default
    );
}

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

public delegate Task RequestHandlerDelegate();