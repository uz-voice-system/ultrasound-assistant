using System.ComponentModel;

namespace UltrasoundAssistant.Contracts.Enums;

/// <summary>
/// Статус записи на приём.
/// </summary>
public enum AppointmentStatus
{
    /// <summary>
    /// Запланирована.
    /// </summary>
    [Description("Запланирована")]
    Scheduled = 0,

    /// <summary>
    /// Приём начат.
    /// </summary>
    [Description("В процессе")]
    InProgress = 1,

    /// <summary>
    /// Приём завершён.
    /// </summary>
    [Description("Завершена")]
    Completed = 2,

    /// <summary>
    /// Запись отменена.
    /// </summary>
    [Description("Отменена")]
    Canceled = 3,

    /// <summary>
    /// Пациент не явился.
    /// </summary>
    [Description("Не явился")]
    NoShow = 4
}
