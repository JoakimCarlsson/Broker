namespace Broker.Example.Features.Generic;

internal sealed record GetGenericCommand<T> : IRequest<T>;

internal sealed class GenericHandler<T> : IHandler<GetGenericCommand<T>, T>
{
    public Task<T> HandleAsync(
        GetGenericCommand<T> request,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }
}