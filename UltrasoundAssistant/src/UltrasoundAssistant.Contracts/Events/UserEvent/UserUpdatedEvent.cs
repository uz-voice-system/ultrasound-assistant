using UltrasoundAssistant.Contracts.Entity.Users;
using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.UserEvent;

/// <summary>
/// Событие обновления пользователя
/// </summary>
public sealed class UserUpdatedEvent : IEvent
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
    /// Логин пользователя
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Хэш пароля
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// ФИО пользователя
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Признак активности пользователя
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Профиль врача
    /// </summary>
    public DoctorProfileDto? DoctorProfile { get; set; }

    /// <summary>
    /// Версия агрегата
    /// </summary>
    public int Version { get; set; }
}
