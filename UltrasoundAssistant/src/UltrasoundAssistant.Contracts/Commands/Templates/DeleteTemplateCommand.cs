namespace UltrasoundAssistant.Contracts.Commands.Templates;

public sealed class DeleteTemplateCommand
{
    public Guid CommandId { get; set; }
    public Guid TemplateId { get; set; }
    public int ExpectedVersion { get; set; }
}
