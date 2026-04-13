namespace UltrasoundAssistant.AggregationService.Infrastructure.Persistence.Entities;

public sealed class EventEntity
{
    public long Id { get; set; }

    public string AggregateType { get; set; } = string.Empty;

    public Guid AggregateId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public int Version { get; set; }

    public string RoutingKey { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}