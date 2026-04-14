namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

public sealed class TemplateReadModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string StructureJson { get; set; } = "{}";

    public bool IsDeleted { get; set; }

    public int Version { get; set; }

    public ICollection<TemplateKeywordReadModel> Keywords { get; set; } = new List<TemplateKeywordReadModel>();
}