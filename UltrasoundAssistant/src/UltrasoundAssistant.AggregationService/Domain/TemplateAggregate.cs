using System.Text.Json;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;
using UltrasoundAssistant.Contracts.Entity.Templates;
using UltrasoundAssistant.Contracts.Events.TemplateEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class TemplateAggregate
{
    public Guid Id { get; private set; }

    public bool Exists { get; private set; }

    public bool IsDeleted { get; private set; }

    public int Version { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public List<TemplateBlockEventDto> Blocks { get; private set; } = [];

    public TemplateCreatedEvent Create(
        Guid templateId,
        string name,
        IReadOnlyList<TemplateBlockEventDto> blocks)
    {
        if (Exists)
            throw new DomainException("Template already exists");

        return new TemplateCreatedEvent
        {
            TemplateId = templateId,
            Name = name.Trim(),
            Blocks = CloneBlocks(blocks),
            Version = Version + 1
        };
    }

    public TemplateUpdatedEvent Update(
        string? name,
        IReadOnlyList<TemplateBlockEventDto>? blocks)
    {
        if (!Exists || IsDeleted)
            throw new DomainException("Template not found");

        var nextName = string.IsNullOrWhiteSpace(name)
            ? Name
            : name.Trim();

        var nextBlocks = blocks is null
            ? CloneBlocks(Blocks)
            : CloneBlocks(blocks);

        return new TemplateUpdatedEvent
        {
            TemplateId = Id,
            Name = nextName,
            Blocks = nextBlocks,
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
                    var e = JsonSerializer.Deserialize<TemplateCreatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid TemplateCreatedEvent payload");

                    Id = e.TemplateId;
                    Name = e.Name;
                    Blocks = CloneBlocks(e.Blocks);
                    Exists = true;
                    IsDeleted = false;
                    Version = e.Version;
                    break;
                }

            case nameof(TemplateUpdatedEvent):
                {
                    var e = JsonSerializer.Deserialize<TemplateUpdatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid TemplateUpdatedEvent payload");

                    Name = string.IsNullOrWhiteSpace(e.Name)
                        ? Name
                        : e.Name;

                    if (e.Blocks is not null)
                        Blocks = CloneBlocks(e.Blocks);

                    Version = e.Version;
                    break;
                }

            case nameof(TemplateDeletedEvent):
                {
                    var e = JsonSerializer.Deserialize<TemplateDeletedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid TemplateDeletedEvent payload");

                    IsDeleted = true;
                    Version = e.Version;
                    break;
                }
        }
    }

    private static List<TemplateBlockEventDto> CloneBlocks(
        IReadOnlyList<TemplateBlockEventDto> blocks)
    {
        return blocks
            .Select(block => new TemplateBlockEventDto
            {
                Id = block.Id,
                Name = block.Name.Trim(),
                Position = block.Position,
                DefaultFieldName = string.IsNullOrWhiteSpace(block.DefaultFieldName)
                    ? null
                    : block.DefaultFieldName.Trim(),
                Phrases = block.Phrases
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                Fields = block.Fields
                    .Select(field => new TemplateFieldEventDto
                    {
                        Id = field.Id,
                        FieldName = field.FieldName.Trim(),
                        DisplayName = field.DisplayName.Trim(),
                        Position = field.Position,
                        Type = field.Type,
                        Phrases = field.Phrases
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(x => x.Trim())
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList(),
                        Norm = field.Norm is null
                            ? null
                            : new FieldNormDto
                            {
                                Min = field.Norm.Min,
                                Max = field.Norm.Max,
                                Unit = string.IsNullOrWhiteSpace(field.Norm.Unit)
                                    ? null
                                    : field.Norm.Unit.Trim(),
                                NormalText = string.IsNullOrWhiteSpace(field.Norm.NormalText)
                                    ? null
                                    : field.Norm.NormalText.Trim()
                            }
                    })
                    .ToList()
            })
            .ToList();
    }
}
