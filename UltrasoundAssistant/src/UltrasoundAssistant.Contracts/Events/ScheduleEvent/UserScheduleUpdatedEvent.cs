using UltrasoundAssistant.Contracts.Entity.Schedules;
using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.ScheduleEvent;

/// <summary>
/// Событие обновления расписания пользователя
/// </summary>
public sealed class UserScheduleUpdatedEvent : IEvent
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
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Элементы расписания
    /// </summary>
    public List<UserScheduleItemDto> Items { get; set; } = [];

    /// <summary>
    /// Версия агрегата
    /// </summary>
    public int Version { get; set; }
}
