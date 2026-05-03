namespace UltrasoundAssistant.Contracts.Events.ScheduleEvent;

/// <summary>
/// Элемент расписания пользователя в событии
/// </summary>
public sealed class UserScheduleItemEventDto
{
    /// <summary>
    /// Идентификатор элемента расписания
    /// </summary>
    public Guid ScheduleId { get; set; }

    /// <summary>
    /// День недели
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Время начала
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Время окончания
    /// </summary>
    public TimeSpan EndTime { get; set; }
}
