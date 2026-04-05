namespace UltrasoundAssistant.Contracts.Commands.Templates;

public sealed class CreateTemplateCommand
{
    public Guid CommandId { get; set; }
    public Guid TemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Keywords { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
