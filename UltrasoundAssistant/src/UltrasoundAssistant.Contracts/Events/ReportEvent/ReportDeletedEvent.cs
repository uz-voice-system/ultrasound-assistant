using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.ReportEvent;

/// <summary>
/// Событие удаления отчёта
/// </summary>
public sealed class ReportDeletedEvent : IEvent
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
    /// Идентификатор отчёта
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Дата обновления отчёта
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Версия агрегата
    /// </summary>
    public int Version { get; set; }
}
