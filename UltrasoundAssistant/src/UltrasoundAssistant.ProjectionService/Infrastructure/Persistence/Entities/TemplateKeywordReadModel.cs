namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

public sealed class TemplateKeywordReadModel
{
    public long Id { get; set; }

    public Guid TemplateId { get; set; }

    public string Phrase { get; set; } = string.Empty;

    public string TargetField { get; set; } = string.Empty;

    public TemplateReadModel Template { get; set; } = null!;
}