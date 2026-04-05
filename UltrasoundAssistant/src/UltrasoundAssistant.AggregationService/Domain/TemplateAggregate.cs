using UltrasoundAssistant.AggregationService.Infrastructure;
using UltrasoundAssistant.Contracts.Events.TemplateEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class TemplateAggregate
{
    public Guid Id { get; private set; }
    public bool Exists { get; private set; }
    public bool IsDeleted { get; private set; }
    public int Version { get; private set; }
    public Dictionary<string, string> Keywords { get; } = new(StringComparer.OrdinalIgnoreCase);

    public void LoadFrom(IEnumerable<EventRecordEnvelope> events)
    {
        foreach (var item in events)
        {
            Apply(item.EventType, item.Payload, item.Version);
        }
    }

    public TemplateCreatedEvent Create(Guid templateId, string name, Dictionary<string, string> keywords)
    {
        if (Exists)
        {
            throw new DomainException("Template already exists");
        }

        if (templateId == Guid.Empty || string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Invalid template data");
        }

        ValidateKeywords(keywords);

        return new TemplateCreatedEvent
        {
            TemplateId = templateId,
            Name = name.Trim(),
            Keywords = NormalizeKeywords(keywords),
            Version = Version + 1
        };
    }

    public TemplateDeletedEvent Delete()
    {
        if (!Exists || IsDeleted)
        {
            throw new DomainException("Template not found");
        }

        return new TemplateDeletedEvent
        {
            TemplateId = Id,
            Version = Version + 1
        };
    }

    public TemplateUpdatedEvent Update(string? name, Dictionary<string, string>? keywords)
    {
        if (!Exists || IsDeleted)
        {
            throw new DomainException("Template not found");
        }

        if (string.IsNullOrWhiteSpace(name) && (keywords is null || keywords.Count == 0))
        {
            throw new DomainException("At least one field should be updated");
        }

        if (keywords is not null)
        {
            ValidateKeywords(keywords);
        }

        return new TemplateUpdatedEvent
        {
            TemplateId = Id,
            Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim(),
            Keywords = keywords is null ? null : NormalizeKeywords(keywords),
            Version = Version + 1
        };
    }

    private void Apply(string eventType, string payload, int version)
    {
        switch (eventType)
        {
            case nameof(TemplateCreatedEvent):
                var created = System.Text.Json.JsonSerializer.Deserialize<TemplateCreatedEvent>(payload)
                    ?? throw new DomainException("Failed to deserialize TemplateCreatedEvent");
                Id = created.TemplateId;
                Exists = true;
                Keywords.Clear();
                foreach (var kv in created.Keywords)
                {
                    Keywords[kv.Key] = kv.Value;
                }
                IsDeleted = false;
                Version = version;
                break;
            case nameof(TemplateUpdatedEvent):
                var updated = System.Text.Json.JsonSerializer.Deserialize<TemplateUpdatedEvent>(payload)
                    ?? throw new DomainException("Failed to deserialize TemplateUpdatedEvent");
                if (updated.Keywords is not null)
                {
                    Keywords.Clear();
                    foreach (var kv in updated.Keywords)
                    {
                        Keywords[kv.Key] = kv.Value;
                    }
                }
                Version = version;
                break;
            case nameof(TemplateDeletedEvent):
                IsDeleted = true;
                Version = version;
                break;
        }
    }

    private static void ValidateKeywords(Dictionary<string, string> keywords)
    {
        if (keywords.Count == 0)
        {
            throw new DomainException("Template keywords cannot be empty");
        }

        if (keywords.Any(x => string.IsNullOrWhiteSpace(x.Key) || string.IsNullOrWhiteSpace(x.Value)))
        {
            throw new DomainException("Template keywords should contain non-empty keys and values");
        }
    }

    private static Dictionary<string, string> NormalizeKeywords(Dictionary<string, string> source)
    {
        return source.ToDictionary(
            k => k.Key.Trim(),
            v => v.Value.Trim(),
            StringComparer.OrdinalIgnoreCase);
    }
}
