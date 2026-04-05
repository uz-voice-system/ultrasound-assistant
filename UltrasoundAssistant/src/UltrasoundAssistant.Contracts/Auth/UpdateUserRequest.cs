using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.Contracts.Auth;

public sealed class UpdateUserRequest
{
    public int ExpectedVersion { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public UserRole? Role { get; set; }
}
