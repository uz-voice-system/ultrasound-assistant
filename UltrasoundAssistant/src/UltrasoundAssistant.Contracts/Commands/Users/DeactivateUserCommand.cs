namespace UltrasoundAssistant.Contracts.Commands.Users;

public sealed class DeactivateUserCommand
{
    public Guid UserId { get; set; }
    public int ExpectedVersion { get; set; }
}
