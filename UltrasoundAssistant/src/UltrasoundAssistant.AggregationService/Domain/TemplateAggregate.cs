using System.Text.Json;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;
using UltrasoundAssistant.Contracts.Events.TemplateEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class TemplateAggregate
{
    public Guid Id { get; private set; }
    public bool Exists { get; private set; }
    public bool IsDeleted { get; private set; }
    public int Version { get; private set; }

    public string Name { get; private set; } = string.Empty;

    // phrase -> fieldName
    public Dictionary<string, string> Keywords { get; private set; } =
        new(StringComparer.OrdinalIgnoreCase);

    public TemplateCreatedEvent Create(Guid templateId, string name, Dictionary<string, string> keywords)
    {
        if (templateId == Guid.Empty)
            throw new DomainException("Template id is required");

        if (Exists)
            throw new DomainException("Template already exists");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Template name is required");

        if (keywords is null || keywords.Count == 0)
            throw new DomainException("Template keywords are required");

        ValidateKeywords(keywords);

        return new TemplateCreatedEvent
        {
            TemplateId = templateId,
            Name = name.Trim(),
            Keywords = new Dictionary<string, string>(keywords, StringComparer.OrdinalIgnoreCase),
            Version = Version + 1
        };
    }

    public TemplateUpdatedEvent Update(string? name, Dictionary<string, string>? keywords)
    {
        if (!Exists || IsDeleted)
            throw new DomainException("Template not found");

        var nextName = string.IsNullOrWhiteSpace(name) ? Name : name.Trim();
        var nextKeywords = keywords is null || keywords.Count == 0
            ? new Dictionary<string, string>(Keywords, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(keywords, StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(nextName))
            throw new DomainException("Template name is required");

        if (nextKeywords.Count == 0)
            throw new DomainException("Template keywords are required");

        ValidateKeywords(nextKeywords);

        return new TemplateUpdatedEvent
        {
            TemplateId = Id,
            Name = nextName,
            Keywords = nextKeywords,
            Version = Version + 1
        };
    }

    public TemplateDeletedEvent Delete()
    {
        if (!Exists || IsDeleted)
            throw new DomainException("Template not found or already deleted");

        return new TemplateDeletedEvent
        {
            TemplateId = Id,
            Version = Version + 1
        };
    }

    public void LoadFrom(IEnumerable<EventRecord> history)
    {
        foreach (var item in history.OrderBy(x => x.Version))
        {
            Apply(item);
        }
    }

    private void Apply(EventRecord record)
    {
        switch (record.EventType)
        {
            case nameof(TemplateCreatedEvent):
                {
                    var e = JsonSerializer.Deserialize<TemplateCreatedEvent>(record.Payload, JsonDefaults.Web)!;
                    Id = e.TemplateId;
                    Name = e.Name;
                    Keywords = new Dictionary<string, string>(e.Keywords, StringComparer.OrdinalIgnoreCase);
                    Exists = true;
                    IsDeleted = false;
                    Version = e.Version;
                    break;
                }

            case nameof(TemplateUpdatedEvent):
                {
                    var e = JsonSerializer.Deserialize<TemplateUpdatedEvent>(record.Payload, JsonDefaults.Web)!;
                    Name = string.IsNullOrWhiteSpace(e.Name) ? Name : e.Name;
                    Keywords = e.Keywords is null || e.Keywords.Count == 0
                        ? Keywords
                        : new Dictionary<string, string>(e.Keywords, StringComparer.OrdinalIgnoreCase);
                    Version = e.Version;
                    break;
                }

            case nameof(TemplateDeletedEvent):
                {
                    var e = JsonSerializer.Deserialize<TemplateDeletedEvent>(record.Payload, JsonDefaults.Web)!;
                    IsDeleted = true;
                    Version = e.Version;
                    break;
                }
        }
    }

    private static void ValidateKeywords(Dictionary<string, string> keywords)
    {
        foreach (var pair in keywords)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
                throw new DomainException("Keyword phrase cannot be empty");

            if (string.IsNullOrWhiteSpace(pair.Value))
                throw new DomainException("Keyword target field cannot be empty");
        }
    }
}