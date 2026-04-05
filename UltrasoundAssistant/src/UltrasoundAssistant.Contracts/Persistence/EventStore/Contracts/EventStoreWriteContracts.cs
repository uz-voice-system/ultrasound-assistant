namespace UltrasoundAssistant.Contracts.Persistence.EventStore.Contracts;

public sealed class EventStoreAppendRequest
{
    public Guid CommandId { get; set; }
    public string CommandType { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public Guid AggregateId { get; set; }
    public int ExpectedVersion { get; set; }
    public List<EventStoreAppendItem> Events { get; set; } = [];
}

public sealed class EventStoreAppendItem
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
