namespace UltrasoundAssistant.AggregationService.Infrastructure.Persistence;

public sealed class EventRecord
{
    public Guid EventId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public int Version { get; init; }
    public string RoutingKey { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}
