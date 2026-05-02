namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Templates;

public sealed class TemplateBlockPhraseReadModel
{
    public long Id { get; set; }

    public Guid BlockId { get; set; }

    public string Phrase { get; set; } = string.Empty;

    public TemplateBlockReadModel Block { get; set; } = null!;
}
