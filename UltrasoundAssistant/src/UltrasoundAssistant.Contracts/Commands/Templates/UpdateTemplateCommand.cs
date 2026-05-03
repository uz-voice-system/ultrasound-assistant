using UltrasoundAssistant.Contracts.Entity.Templates;

namespace UltrasoundAssistant.Contracts.Commands.Templates;

public sealed class UpdateTemplateCommand
{
    public Guid TemplateId { get; set; }

    public int ExpectedVersion { get; set; }

    public string? Name { get; set; }

    public List<TemplateBlockDto>? Blocks { get; set; }
}
