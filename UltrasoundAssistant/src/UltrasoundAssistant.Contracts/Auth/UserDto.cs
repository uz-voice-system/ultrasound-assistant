using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.Contracts.Auth
{
    /// <summary>
    /// DTO пользователя системы.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Уникальный идентификатор пользователя.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Полное имя пользователя.
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Роль пользователя в системе.
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Активен ли пользователь (soft delete = false).
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Версия агрегата для optimistic concurrency.
        /// </summary>
        public int Version { get; set; }
    }
}
