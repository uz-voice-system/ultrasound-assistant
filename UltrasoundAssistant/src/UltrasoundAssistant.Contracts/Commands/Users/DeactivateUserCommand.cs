namespace UltrasoundAssistant.Contracts.Commands.Users;

public sealed class DeactivateUserCommand
{
    public Guid CommandId { get; set; }
    public Guid UserId { get; set; }
    public int ExpectedVersion { get; set; }
}
