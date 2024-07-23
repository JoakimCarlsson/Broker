# Broker

A lightweight, flexible framework for handling requests and responses in a pipeline-oriented fashion, targeting .NET Standard 2.0.

## Projects

### Broker.Abstractions

Contains core interfaces and abstractions:

- **Handlers**:
  - `IHandler<TRequest, TResponse>`: Handles requests that produce a response.
  - `IHandler<TRequest>`: Handles requests that do not produce a response.

- **Requests**:
  - `IRequest`: Represents a request with no expected response.
  - `IRequest<TResponse>`: Represents a request that expects a response of type `TResponse`.

- **Pipeline Behaviors**:
  - `IRequestPipelineBehavior<TRequest, TResponse>`: Defines a behavior to be executed around a request and its response.
  - `IRequestPipelineBehavior<TRequest>`: Defines a behavior to be executed around a request with no response.

- **Pre/Post Processors**:
  - `IRequestPreProcessor<TRequest>`: Executes logic before a request is handled.
  - `IRequestPostProcessor<TRequest, TResponse>`: Executes logic after a request is handled and produces a response.
  - `IRequestPostProcessor<TRequest>`: Executes logic after a request is handled when there is no response.

- **Sender**:
  - `ISender`: Sends requests and optionally returns responses.

### Broker.SourceGenerator

Automatically generates the `Sender` implementation and registers all handlers, pre-processors, post-processors, and pipeline behaviors in the DI container.
