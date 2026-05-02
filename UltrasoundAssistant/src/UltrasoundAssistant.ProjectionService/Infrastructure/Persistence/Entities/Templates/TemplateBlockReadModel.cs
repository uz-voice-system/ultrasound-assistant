namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Templates;

public sealed class TemplateBlockReadModel
{
    public Guid Id { get; set; }

    public Guid TemplateId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Position { get; set; }

    public string? DefaultFieldName { get; set; }

    public TemplateReadModel Template { get; set; } = null!;

    public ICollection<TemplateBlockPhraseReadModel> Phrases { get; set; } = new List<TemplateBlockPhraseReadModel>();

    public ICollection<TemplateFieldReadModel> Fields { get; set; } = new List<TemplateFieldReadModel>();
}
