using UltrasoundAssistant.Contracts.Persistence.EventStore.Contracts;

namespace UltrasoundAssistant.AggregationService.Infrastructure;

public interface IEventStore
{
    Task<IReadOnlyList<EventRecordEnvelope>> LoadAggregateEventsAsync(string aggregateType, Guid aggregateId, CancellationToken cancellationToken);
    Task<AppendResult> AppendWithOutboxAsync(EventStoreAppendRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<OutboxMessage>> ReadPendingOutboxAsync(int batchSize, CancellationToken cancellationToken);
    Task MarkOutboxPublishedAsync(long outboxId, CancellationToken cancellationToken);
    Task MarkOutboxFailedAsync(long outboxId, CancellationToken cancellationToken);
}
