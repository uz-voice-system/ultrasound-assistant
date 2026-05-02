using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.TemplateEvent;

/// <summary>
/// Событие создания шаблона
/// </summary>
public sealed class TemplateCreatedEvent : IEvent
{
    /// <summary>
    /// Идентификатор события
    /// </summary>
    public Guid EventId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Дата создания события
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Идентификатор шаблона
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Название шаблона
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Блоки шаблона
    /// </summary>
    public List<TemplateBlockEventDto> Blocks { get; set; } = [];

    /// <summary>
    /// Версия шаблона
    /// </summary>
    public int Version { get; set; } = 1;
}
