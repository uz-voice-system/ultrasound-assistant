using UltrasoundAssistant.Contracts.Entity.Templates;
using UltrasoundAssistant.Contracts.Events.TemplateEvent;
using UltrasoundAssistant.Contracts.Reads.Templates.Admin;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.Contracts.Reads.Templates.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Templates;

namespace UltrasoundAssistant.ProjectionService.Application.Mapping;

public sealed class TemplateProjectionMapper
{
    public List<TemplateBlockReadModel> MapBlocks(Guid templateId, IReadOnlyList<TemplateBlockEventDto> blocks)
    {
        return blocks
            .OrderBy(x => x.Position)
            .Select(block =>
            {
                var blockReadModel = new TemplateBlockReadModel
                {
                    Id = block.Id,
                    TemplateId = templateId,
                    Name = block.Name,
                    Position = block.Position,
                    DefaultFieldName = block.DefaultFieldName
                };

                blockReadModel.Phrases = block.Phrases
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(phrase => new TemplateBlockPhraseReadModel
                    {
                        BlockId = blockReadModel.Id,
                        Phrase = phrase
                    })
                    .ToList();

                blockReadModel.Fields = block.Fields
                    .OrderBy(x => x.Position)
                    .Select(field =>
                    {
                        var fieldReadModel = new TemplateFieldReadModel
                        {
                            Id = field.Id,
                            BlockId = blockReadModel.Id,
                            FieldName = field.FieldName,
                            DisplayName = field.DisplayName,
                            Position = field.Position,
                            Type = field.Type,
                            NormMin = field.Norm?.Min,
                            NormMax = field.Norm?.Max,
                            NormUnit = field.Norm?.Unit,
                            NormNormalText = field.Norm?.NormalText
                        };

                        fieldReadModel.Phrases = field.Phrases
                            .Select(x => x.Trim())
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Select(phrase => new TemplateFieldPhraseReadModel
                            {
                                FieldId = fieldReadModel.Id,
                                Phrase = phrase
                            })
                            .ToList();

                        return fieldReadModel;
                    })
                    .ToList();

                return blockReadModel;
            })
            .ToList();
    }

    public TemplateSummaryDto MapSummary(TemplateReadModel template)
    {
        return new TemplateSummaryDto
        {
            Id = template.Id,
            Name = template.Name,
            IsDeleted = template.IsDeleted,
            Version = template.Version
        };
    }

    public TemplateDto MapFull(TemplateReadModel template)
    {
        return new TemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            IsDeleted = template.IsDeleted,
            Version = template.Version,
            Blocks = template.Blocks
                .OrderBy(x => x.Position)
                .Select(MapBlock)
                .ToList()
        };
    }

    public TemplateAdminSearchResultDto MapAdminSearchResult(TemplateReadModel template, TemplateAdminSearchRequest filter)
    {
        return new TemplateAdminSearchResultDto
        {
            Template = MapFull(template),
            Matches = BuildMatches(template, filter)
        };
    }

    private static TemplateBlockDto MapBlock(TemplateBlockReadModel block)
    {
        return new TemplateBlockDto
        {
            Id = block.Id,
            Name = block.Name,
            Position = block.Position,
            DefaultFieldName = block.DefaultFieldName,
            Phrases = block.Phrases
                .OrderBy(x => x.Phrase)
                .Select(x => x.Phrase)
                .ToList(),
            Fields = block.Fields
                .OrderBy(x => x.Position)
                .Select(MapField)
                .ToList()
        };
    }

    private static TemplateFieldDto MapField(TemplateFieldReadModel field)
    {
        return new TemplateFieldDto
        {
            Id = field.Id,
            FieldName = field.FieldName,
            DisplayName = field.DisplayName,
            Position = field.Position,
            Type = field.Type,
            Phrases = field.Phrases
                .OrderBy(x => x.Phrase)
                .Select(x => x.Phrase)
                .ToList(),
            Norm = HasNorm(field)
                ? new FieldNormDto
                {
                    Min = field.NormMin,
                    Max = field.NormMax,
                    Unit = field.NormUnit,
                    NormalText = field.NormNormalText
                }
                : null
        };
    }

    private static bool HasNorm(TemplateFieldReadModel field)
    {
        return field.NormMin is not null ||
               field.NormMax is not null ||
               !string.IsNullOrWhiteSpace(field.NormUnit) ||
               !string.IsNullOrWhiteSpace(field.NormNormalText);
    }

    private static List<TemplateSearchMatchDto> BuildMatches(
        TemplateReadModel template,
        TemplateAdminSearchRequest filter)
    {
        var matches = new List<TemplateSearchMatchDto>();

        AddGeneralMatches(template, filter.SearchText, matches);

        if (!string.IsNullOrWhiteSpace(filter.TemplateName))
            AddTemplateNameMatch(template, filter.TemplateName, matches);

        if (!string.IsNullOrWhiteSpace(filter.BlockName))
            AddBlockNameMatches(template, filter.BlockName, matches);

        if (!string.IsNullOrWhiteSpace(filter.FieldName))
            AddFieldNameMatches(template, filter.FieldName, matches);

        if (!string.IsNullOrWhiteSpace(filter.FieldDisplayName))
            AddFieldDisplayNameMatches(template, filter.FieldDisplayName, matches);

        if (!string.IsNullOrWhiteSpace(filter.Phrase))
            AddPhraseMatches(template, filter.Phrase, matches);

        if (filter.FieldType is not null)
            AddFieldTypeMatches(template, filter.FieldType.Value, matches);

        if (filter.HasNorm is not null)
            AddNormMatches(template, filter.HasNorm.Value, matches);

        return matches
            .DistinctBy(x => new
            {
                x.Type,
                x.Value,
                x.BlockId,
                x.FieldId
            })
            .ToList();
    }

    private static void AddGeneralMatches(
        TemplateReadModel template,
        string? searchText,
        List<TemplateSearchMatchDto> matches)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return;

        AddTemplateNameMatch(template, searchText, matches);
        AddBlockNameMatches(template, searchText, matches);
        AddFieldNameMatches(template, searchText, matches);
        AddFieldDisplayNameMatches(template, searchText, matches);
        AddPhraseMatches(template, searchText, matches);
    }

    private static void AddTemplateNameMatch(
        TemplateReadModel template,
        string searchText,
        List<TemplateSearchMatchDto> matches)
    {
        if (!Contains(template.Name, searchText)) return;

        matches.Add(new TemplateSearchMatchDto
        {
            Type = TemplateSearchMatchType.TemplateName,
            Value = template.Name
        });
    }

    private static void AddBlockNameMatches(
        TemplateReadModel template,
        string searchText,
        List<TemplateSearchMatchDto> matches)
    {
        foreach (var block in template.Blocks)
        {
            if (!Contains(block.Name, searchText)) continue;

            matches.Add(new TemplateSearchMatchDto
            {
                Type = TemplateSearchMatchType.BlockName,
                Value = block.Name,
                BlockId = block.Id,
                BlockName = block.Name
            });
        }
    }

    private static void AddFieldNameMatches(
        TemplateReadModel template,
        string searchText,
        List<TemplateSearchMatchDto> matches)
    {
        foreach (var block in template.Blocks)
        {
            foreach (var field in block.Fields)
            {
                if (!Contains(field.FieldName, searchText)) continue;

                matches.Add(CreateFieldMatch(
                    TemplateSearchMatchType.FieldName,
                    field.FieldName,
                    block,
                    field));
            }
        }
    }

    private static void AddFieldDisplayNameMatches(
        TemplateReadModel template,
        string searchText,
        List<TemplateSearchMatchDto> matches)
    {
        foreach (var block in template.Blocks)
        {
            foreach (var field in block.Fields)
            {
                if (!Contains(field.DisplayName, searchText)) continue;

                matches.Add(CreateFieldMatch(
                    TemplateSearchMatchType.FieldDisplayName,
                    field.DisplayName,
                    block,
                    field));
            }
        }
    }

    private static void AddPhraseMatches(
        TemplateReadModel template,
        string searchText,
        List<TemplateSearchMatchDto> matches)
    {
        foreach (var block in template.Blocks)
        {
            foreach (var phrase in block.Phrases)
            {
                if (!Contains(phrase.Phrase, searchText))
                    continue;

                matches.Add(new TemplateSearchMatchDto
                {
                    Type = TemplateSearchMatchType.BlockPhrase,
                    Value = phrase.Phrase,
                    BlockId = block.Id,
                    BlockName = block.Name
                });
            }

            foreach (var field in block.Fields)
            {
                foreach (var phrase in field.Phrases)
                {
                    if (!Contains(phrase.Phrase, searchText))
                        continue;

                    matches.Add(CreateFieldMatch(
                        TemplateSearchMatchType.FieldPhrase,
                        phrase.Phrase,
                        block,
                        field));
                }
            }
        }
    }

    private static void AddFieldTypeMatches(
        TemplateReadModel template,
        TemplateFieldType fieldType,
        List<TemplateSearchMatchDto> matches)
    {
        foreach (var block in template.Blocks)
        {
            foreach (var field in block.Fields.Where(x => x.Type == fieldType))
            {
                matches.Add(CreateFieldMatch(
                    TemplateSearchMatchType.FieldType,
                    field.Type.ToString(),
                    block,
                    field));
            }
        }
    }

    private static void AddNormMatches(
        TemplateReadModel template,
        bool hasNorm,
        List<TemplateSearchMatchDto> matches)
    {
        foreach (var block in template.Blocks)
        {
            foreach (var field in block.Fields)
            {
                var fieldHasNorm = HasNorm(field);

                if (fieldHasNorm != hasNorm)
                    continue;

                matches.Add(CreateFieldMatch(
                    TemplateSearchMatchType.FieldNorm,
                    hasNorm ? "Норма задана" : "Норма не задана",
                    block,
                    field));
            }
        }
    }

    private static TemplateSearchMatchDto CreateFieldMatch(
        TemplateSearchMatchType type,
        string value,
        TemplateBlockReadModel block,
        TemplateFieldReadModel field)
    {
        return new TemplateSearchMatchDto
        {
            Type = type,
            Value = value,
            BlockId = block.Id,
            BlockName = block.Name,
            FieldId = field.Id,
            FieldName = field.FieldName,
            FieldDisplayName = field.DisplayName
        };
    }

    private static bool Contains(string source, string searchText)
    {
        return source.Contains(searchText.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
