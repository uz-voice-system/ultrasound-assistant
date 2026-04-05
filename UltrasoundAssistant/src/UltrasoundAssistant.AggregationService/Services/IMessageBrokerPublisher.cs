namespace UltrasoundAssistant.AggregationService.Services;

public interface IMessageBrokerPublisher
{
    Task PublishAsync(string exchange, string routingKey, string eventType, string payload, CancellationToken cancellationToken);
}
