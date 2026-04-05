using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.Contracts.Auth;

public sealed class CreateUserRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
