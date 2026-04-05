namespace UltrasoundAssistant.ProjectionService.Persistence.Entities;

public sealed class KeywordReadEntity
{
    public long Id { get; set; }

    public Guid TemplateId { get; set; }

    public TemplateReadEntity? Template { get; set; }

    public string Phrase { get; set; } = string.Empty;

    public string TargetField { get; set; } = string.Empty;
}
