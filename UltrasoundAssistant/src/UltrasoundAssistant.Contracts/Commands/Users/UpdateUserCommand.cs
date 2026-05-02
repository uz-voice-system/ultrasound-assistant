using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.Contracts.Commands.Users;

public sealed class UpdateUserCommand
{
    public Guid UserId { get; set; }
    public int ExpectedVersion { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public UserRole? Role { get; set; }
}
