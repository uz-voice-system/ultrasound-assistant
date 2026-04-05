namespace UltrasoundAssistant.AggregationService.Persistence.Entities;

/// <summary>
/// Append-only запись доменного события в Event Store.
/// </summary>
public sealed class StoredEventEntity
{
    public long Id { get; set; }

    public Guid EventId { get; set; }

    public Guid CommandId { get; set; }

    public string AggregateType { get; set; } = string.Empty;

    public Guid AggregateId { get; set; }

    public int Version { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
