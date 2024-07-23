namespace Broker.Abstractions;

public interface IRequestPreProcessor<in TRequest>
{
    Task ProcessAsync(
        TRequest request,
        CancellationToken cancellationToken = default
    );
}