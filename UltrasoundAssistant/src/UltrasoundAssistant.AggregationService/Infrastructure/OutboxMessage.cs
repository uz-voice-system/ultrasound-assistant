namespace UltrasoundAssistant.AggregationService.Infrastructure;

public sealed class OutboxMessage
{
    public long Id { get; init; }
    public Guid EventId { get; init; }
    public string Exchange { get; init; } = string.Empty;
    public string RoutingKey { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public int Attempts { get; init; }
}
