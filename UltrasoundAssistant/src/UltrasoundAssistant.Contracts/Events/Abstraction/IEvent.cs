

namespace UltrasoundAssistant.Contracts.Events.Abstraction
{
    /// <summary>
    /// Базовый интерфейс доменного события.
    /// Используется для передачи событий между сервисами.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Уникальный идентификатор события.
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// Дата и время создания события (UTC).
        /// </summary>
        DateTime CreatedAt { get; }
    }
}
