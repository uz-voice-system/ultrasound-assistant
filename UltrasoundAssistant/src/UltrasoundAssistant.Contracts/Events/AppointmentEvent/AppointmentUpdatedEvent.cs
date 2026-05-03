using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.AppointmentEvent;

/// <summary>
/// Событие обновления записи на приём
/// </summary>
public sealed class AppointmentUpdatedEvent : IEvent
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
    /// Идентификатор записи
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Идентификатор пациента
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Идентификатор врача
    /// </summary>
    public Guid DoctorId { get; set; }

    /// <summary>
    /// Идентификатор шаблона исследования
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Начало приёма
    /// </summary>
    public DateTime StartAtUtc { get; set; }

    /// <summary>
    /// Окончание приёма
    /// </summary>
    public DateTime EndAtUtc { get; set; }

    /// <summary>
    /// Статус записи
    /// </summary>
    public AppointmentStatus Status { get; set; }

    /// <summary>
    /// Комментарий к записи
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Дата обновления записи
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Версия агрегата
    /// </summary>
    public int Version { get; set; }
}
