using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.TemplateEvent;

public sealed class TemplateUpdatedEvent : IEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid TemplateId { get; set; }
    public string? Name { get; set; }
    public Dictionary<string, string>? Keywords { get; set; }
    public int Version { get; set; }
}
