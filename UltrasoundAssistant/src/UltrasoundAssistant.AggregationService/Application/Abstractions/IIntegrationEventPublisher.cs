using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;

namespace UltrasoundAssistant.AggregationService.Application.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(string aggregateType, Guid aggregateId, IReadOnlyList<EventRecord> events, CancellationToken cancellationToken);
}