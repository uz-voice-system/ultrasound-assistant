namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Templates;

public sealed class TemplateFieldPhraseReadModel
{
    public long Id { get; set; }

    public Guid FieldId { get; set; }

    public string Phrase { get; set; } = string.Empty;

    public TemplateFieldReadModel Field { get; set; } = null!;
}
