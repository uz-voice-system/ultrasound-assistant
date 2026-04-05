namespace UltrasoundAssistant.AggregationService.Infrastructure;

public sealed class EventRecordEnvelope
{
    public Guid EventId { get; init; }
    public Guid CommandId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public int Version { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
