using UltrasoundAssistant.Contracts.Entity.Templates;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Templates;

public sealed class TemplateFieldReadModel
{
    public Guid Id { get; set; }

    public Guid BlockId { get; set; }

    public string FieldName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public int Position { get; set; }

    public TemplateFieldType Type { get; set; }

    public TemplateFieldRole Role { get; set; } = TemplateFieldRole.Regular;

    public decimal? NormMin { get; set; }

    public decimal? NormMax { get; set; }

    public string? NormUnit { get; set; }

    public string? NormNormalText { get; set; }

    public TemplateBlockReadModel Block { get; set; } = null!;

    public ICollection<TemplateFieldPhraseReadModel> Phrases { get; set; } = new List<TemplateFieldPhraseReadModel>();
}
