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
    }
}
