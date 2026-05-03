using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.ReportEvent;

/// <summary>
/// Событие создания отчёта
/// </summary>
public sealed class ReportCreatedEvent : IEvent
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
    /// Идентификатор записи на приём
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Статус отчёта
    /// </summary>
    public ReportStatus Status { get; set; }

    /// <summary>
    /// Содержимое отчёта в формате JSON
    /// </summary>
    public string ContentJson { get; set; } = "{}";

    /// <summary>
    /// Дата создания отчёта
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Дата обновления отчёта
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Версия агрегата
    /// </summary>
    public int Version { get; set; }
}
