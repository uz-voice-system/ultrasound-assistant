namespace UltrasoundAssistant.Contracts.Commands.Templates;

public sealed class DeleteTemplateCommand
{
    public Guid TemplateId { get; set; }

    public int ExpectedVersion { get; set; }
}
