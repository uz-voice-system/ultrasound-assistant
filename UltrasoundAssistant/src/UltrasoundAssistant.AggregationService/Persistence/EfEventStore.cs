using Microsoft.EntityFrameworkCore;
using Npgsql;
using UltrasoundAssistant.AggregationService.Infrastructure;
using UltrasoundAssistant.AggregationService.Persistence.Entities;
using UltrasoundAssistant.Contracts.Persistence.EventStore.Contracts;
using UltrasoundAssistant.Contracts.Persistence.Messaging;

namespace UltrasoundAssistant.AggregationService.Persistence;

public sealed class EfEventStore(EventStoreDbContext db, ILogger<EfEventStore> logger) : IEventStore
{
    public async Task<IReadOnlyList<EventRecordEnvelope>> LoadAggregateEventsAsync(
        string aggregateType,
        Guid aggregateId,
        CancellationToken cancellationToken)
    {
        return await db.StoredEvents
            .AsNoTracking()
            .Where(e => e.AggregateType == aggregateType && e.AggregateId == aggregateId)
            .OrderBy(e => e.Version)
            .Select(e => new EventRecordEnvelope
            {
                EventId = e.EventId,
                CommandId = e.CommandId,
                EventType = e.EventType,
                Payload = e.Payload,
                Version = e.Version,
                CreatedAtUtc = e.CreatedAt.UtcDateTime
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AppendResult> AppendWithOutboxAsync(EventStoreAppendRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            db.ProcessedCommands.Add(new ProcessedCommandEntity
            {
                CommandId = request.CommandId,
                CommandType = request.CommandType,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AppendResult.DuplicateCommand;
            }

            var currentVersion = await db.StoredEvents
                .Where(e => e.AggregateType == request.AggregateType && e.AggregateId == request.AggregateId)
                .Select(e => (int?)e.Version)
                .MaxAsync(cancellationToken) ?? 0;

            if (currentVersion != request.ExpectedVersion)
            {
                await transaction.RollbackAsync(cancellationToken);
                return AppendResult.ConcurrencyConflict;
            }

            var now = DateTimeOffset.UtcNow;
            foreach (var item in request.Events)
            {
                db.StoredEvents.Add(new StoredEventEntity
                {
                    EventId = item.EventId,
                    CommandId = request.CommandId,
                    AggregateType = request.AggregateType,
                    AggregateId = request.AggregateId,
                    Version = item.Version,
                    EventType = item.EventType,
                    Payload = item.Payload,
                    CreatedAt = new DateTimeOffset(item.CreatedAtUtc.ToUniversalTime(), TimeSpan.Zero)
                });

                db.OutboxMessages.Add(new OutboxMessageEntity
                {
                    EventId = item.EventId,
                    ExchangeName = DomainEventExchange.Name,
                    RoutingKey = item.RoutingKey,
                    EventType = item.EventType,
                    Payload = item.Payload,
                    Attempts = 0,
                    PublishedAtUtc = null,
                    NextAttemptAtUtc = now,
                    CreatedAtUtc = now
                });
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return AppendResult.Success;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning(ex, "Concurrency/duplicate conflict for {AggregateType}/{AggregateId}", request.AggregateType, request.AggregateId);
            return AppendResult.ConcurrencyConflict;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<OutboxMessage>> ReadPendingOutboxAsync(int batchSize, CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;
        return await db.OutboxMessages
            .AsNoTracking()
            .Where(o => o.PublishedAtUtc == null && o.NextAttemptAtUtc <= utcNow)
            .OrderBy(o => o.Id)
            .Take(batchSize)
            .Select(o => new OutboxMessage
            {
                Id = o.Id,
                EventId = o.EventId,
                Exchange = o.ExchangeName,
                RoutingKey = o.RoutingKey,
                EventType = o.EventType,
                Payload = o.Payload,
                Attempts = o.Attempts
            })
            .ToListAsync(cancellationToken);
    }

    public async Task MarkOutboxPublishedAsync(long outboxId, CancellationToken cancellationToken)
    {
        var entity = await db.OutboxMessages.FirstOrDefaultAsync(o => o.Id == outboxId, cancellationToken);
        if (entity is null)
            return;

        entity.PublishedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkOutboxFailedAsync(long outboxId, CancellationToken cancellationToken)
    {
        var entity = await db.OutboxMessages.FirstOrDefaultAsync(o => o.Id == outboxId, cancellationToken);
        if (entity is null)
            return;

        entity.Attempts++;
        entity.NextAttemptAtUtc = DateTimeOffset.UtcNow.AddSeconds(10);
        await db.SaveChangesAsync(cancellationToken);
    }
}
