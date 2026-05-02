namespace UltrasoundAssistant.Contracts.Commands.Patients;

/// <summary>
/// Команда на создание пациента.
/// </summary>
public sealed class CreatePatientCommand
{
    /// <summary>
    /// Уникальный идентификатор пациента.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Полное имя пациента.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Дата рождения пациента.
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Пол пациента (опционально).
    /// </summary>
    public string? Gender { get; set; }
}
