using UltrasoundAssistant.Contracts.Entity.Templates;

namespace UltrasoundAssistant.Contracts.Commands.Templates;

public sealed class CreateTemplateCommand
{
    public Guid TemplateId { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<TemplateBlockDto> Blocks { get; set; } = [];
}
