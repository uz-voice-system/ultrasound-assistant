using UltrasoundAssistant.Contracts.Events.TemplateEvent;

namespace UltrasoundAssistant.Contracts.Commands.Templates;

public sealed class CreateTemplateCommand
{
    public Guid TemplateId { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<TemplateBlockEventDto> Blocks { get; set; } = [];
}
