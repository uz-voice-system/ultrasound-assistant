using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.AggregationService.Infrastructure.Persistence;

public sealed class EfCoreEventStore : IEventStore
{
    private readonly AggregationDbContext _dbContext;

    public EfCoreEventStore(AggregationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EventRecord>> LoadAggregateEventsAsync(
        string aggregateType,
        Guid aggregateId,
        CancellationToken cancellationToken)
    {
        var entities = await _dbContext.Events
            .AsNoTracking()
            .Where(x => x.AggregateType == aggregateType && x.AggregateId == aggregateId)
            .OrderBy(x => x.Version)
            .ToListAsync(cancellationToken);

        var items = entities
            .Select(x => new EventRecord
            {
                EventId = Guid.Empty,
                EventType = x.EventType,
                Payload = x.Payload,
                Version = x.Version,
                RoutingKey = x.RoutingKey,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToList();

        return items;
    }

    public async Task AppendEventsAsync(
        string aggregateType,
        Guid aggregateId,
        int expectedVersion,
        IReadOnlyList<EventRecord> events,
        CancellationToken cancellationToken)
    {
        var currentVersion = await _dbContext.Events
            .Where(x => x.AggregateType == aggregateType && x.AggregateId == aggregateId)
            .Select(x => (int?)x.Version)
            .MaxAsync(cancellationToken) ?? 0;

        if (currentVersion != expectedVersion)
            throw new InvalidOperationException(
                $"Concurrency conflict. Expected {expectedVersion}, actual {currentVersion}");

        var entities = events.Select(x => new EventEntity
        {
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            EventType = x.EventType,
            Payload = x.Payload,
            Version = x.Version,
            RoutingKey = x.RoutingKey,
            CreatedAtUtc = x.CreatedAtUtc
        });

        await _dbContext.Events.AddRangeAsync(entities, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}