using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.PatientEvent;

public sealed class PatientDeactivatedEvent : IEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid PatientId { get; set; }
    public string? Reason { get; set; }
    public bool IsDeleted { get; set; } = true;
    public int Version { get; set; }
}
