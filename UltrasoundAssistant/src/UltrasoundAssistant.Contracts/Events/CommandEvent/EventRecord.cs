namespace UltrasoundAssistant.Contracts.Events.CommandEvent;

public sealed class EventRecord
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public int Version { get; set; }
    public string RoutingKey { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}