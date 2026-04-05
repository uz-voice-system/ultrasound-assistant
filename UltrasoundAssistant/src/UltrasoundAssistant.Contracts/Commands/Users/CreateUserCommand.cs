using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.Contracts.Commands.Users;

public sealed class CreateUserCommand
{
    public Guid CommandId { get; set; }
    public Guid UserId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
