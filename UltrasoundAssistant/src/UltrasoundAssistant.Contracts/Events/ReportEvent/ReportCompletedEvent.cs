using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.ReportEvent;

public sealed class ReportCompletedEvent : IEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid ReportId { get; set; }
    public int Version { get; set; }
}
