namespace UltrasoundAssistant.ProjectionService.Application.Abstractions;

public interface IIntegrationEventHandler
{
    string RoutingKey { get; }

    Task HandleAsync(string payload, CancellationToken cancellationToken);
}