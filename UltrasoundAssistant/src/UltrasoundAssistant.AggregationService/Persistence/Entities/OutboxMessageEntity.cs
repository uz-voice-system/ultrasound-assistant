namespace UltrasoundAssistant.AggregationService.Persistence.Entities;

public sealed class OutboxMessageEntity
{
    public long Id { get; set; }

    public Guid EventId { get; set; }

    public string ExchangeName { get; set; } = string.Empty;

    public string RoutingKey { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public int Attempts { get; set; }

    public DateTimeOffset? PublishedAtUtc { get; set; }

    public DateTimeOffset NextAttemptAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
}
