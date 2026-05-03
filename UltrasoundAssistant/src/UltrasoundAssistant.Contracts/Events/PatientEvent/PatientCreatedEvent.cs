using UltrasoundAssistant.Contracts.Entity.Patients;
using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.PatientEvent;

/// <summary>
/// Событие создания пациента
/// </summary>
public sealed class PatientCreatedEvent : IEvent
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
    /// Идентификатор пациента
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// ФИО пациента
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Дата рождения
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Пол
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Номер телефона
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Электронная почта
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Документы пациента
    /// </summary>
    public List<PatientDocumentDto> Documents { get; set; } = [];

    /// <summary>
    /// Версия агрегата
    /// </summary>
    public int Version { get; set; }
}
