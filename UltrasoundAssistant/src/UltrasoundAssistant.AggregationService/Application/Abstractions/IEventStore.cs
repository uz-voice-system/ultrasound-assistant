using UltrasoundAssistant.AggregationService.Infrastructure;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;
using UltrasoundAssistant.Contracts.Persistence.EventStore.Contracts;

namespace UltrasoundAssistant.AggregationService.Application.Abstractions;

public interface IEventStore
{
    Task<IReadOnlyList<EventRecord>> LoadAggregateEventsAsync(string aggregateType, Guid aggregateId, CancellationToken cancellationToken);
    Task AppendEventsAsync(string aggregateType, Guid aggregateId, int expectedVersion, IReadOnlyList<EventRecord> events, CancellationToken cancellationToken);
}
