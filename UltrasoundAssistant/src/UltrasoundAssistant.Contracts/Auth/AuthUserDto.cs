using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.Contracts.Auth;

/// <summary>
/// Пользователь с подтверждёнными учётными данными
/// </summary>
public sealed class AuthUserDto
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Логин пользователя
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// ФИО пользователя
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; set; }
}
