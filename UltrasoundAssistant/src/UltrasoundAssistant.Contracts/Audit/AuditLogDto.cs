

namespace UltrasoundAssistant.Contracts.Audit
{
    /// <summary>
    /// DTO записи аудита действий пользователя в системе.
    /// </summary>
    public class AuditLogDto
    {
        /// <summary>
        /// Уникальный идентификатор записи аудита.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор пользователя, совершившего действие.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Имя пользователя для удобства отображения в логах.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Тип действия (например: LOGIN, VIEW, PRINT).
        /// </summary>
        public string ActionType { get; set; } = null!;

        /// <summary>
        /// Сущность, над которой выполнено действие (например: Report, Patient).
        /// </summary>
        public string Entity { get; set; } = null!;

        /// <summary>
        /// Идентификатор сущности, если применимо.
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Дата и время выполнения действия.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
