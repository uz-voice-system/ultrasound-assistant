namespace UltrasoundAssistant.Contracts.Auth;

public sealed class DeactivateUserRequest
{
    public Guid CommandId { get; set; }
    public int ExpectedVersion { get; set; }
}
