namespace Broker.Abstractions;

public interface IRequest { }

public interface IRequest<out TResponse> { }