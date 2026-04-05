namespace UltrasoundAssistant.Contracts.Templates;

public sealed class DeleteTemplateRequest
{
    public Guid CommandId { get; set; }
    public int ExpectedVersion { get; set; }
}
