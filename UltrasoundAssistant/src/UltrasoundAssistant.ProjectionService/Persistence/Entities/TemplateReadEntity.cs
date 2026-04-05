namespace UltrasoundAssistant.ProjectionService.Persistence.Entities;

public sealed class TemplateReadEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string StructureJson { get; set; } = "{}";

    public int Version { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<KeywordReadEntity> Keywords { get; set; } = new List<KeywordReadEntity>();
}
