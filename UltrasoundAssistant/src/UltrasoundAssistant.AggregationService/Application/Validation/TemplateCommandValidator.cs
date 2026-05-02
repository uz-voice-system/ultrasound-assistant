using System.Text.RegularExpressions;
using UltrasoundAssistant.Contracts.Commands.Templates;
using UltrasoundAssistant.Contracts.Entity.Templates;
using UltrasoundAssistant.Contracts.Events.TemplateEvent;

namespace UltrasoundAssistant.AggregationService.Application.Validation;

public static partial class TemplateCommandValidator
{
    private const int MaxTemplateNameLength = 200;
    private const int MaxBlockNameLength = 200;
    private const int MaxFieldNameLength = 200;
    private const int MaxDisplayNameLength = 200;
    private const int MaxPhraseLength = 300;
    private const int MaxNormUnitLength = 50;
    private const int MaxNormTextLength = 500;

    public static void Validate(CreateTemplateCommand command)
    {
        if (command.TemplateId == Guid.Empty)
            throw new ArgumentException("TemplateId is required");

        ValidateTemplateName(command.Name, required: true);

        ValidateBlocks(command.Blocks);
    }

    public static void Validate(UpdateTemplateCommand command)
    {
        if (command.TemplateId == Guid.Empty)
            throw new ArgumentException("TemplateId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");

        if (command.Name is not null)
            ValidateTemplateName(command.Name, required: false);

        if (command.Blocks is not null)
            ValidateBlocks(command.Blocks);

        if (command.Name is null && command.Blocks is null)
            throw new ArgumentException("Template update data is required");
    }

    public static void Validate(DeleteTemplateCommand command)
    {
        if (command.TemplateId == Guid.Empty)
            throw new ArgumentException("TemplateId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }

    private static void ValidateTemplateName(string? name, bool required)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            if (required)
                throw new ArgumentException("Template name is required");

            if (name is not null)
                throw new ArgumentException("Template name cannot be empty");

            return;
        }

        if (name.Trim().Length > MaxTemplateNameLength)
            throw new ArgumentException($"Template name length cannot exceed {MaxTemplateNameLength}");
    }

    private static void ValidateBlocks(IReadOnlyList<TemplateBlockEventDto>? blocks)
    {
        if (blocks is null || blocks.Count == 0)
            throw new ArgumentException("Template blocks are required");

        var blockIds = new HashSet<Guid>();
        var blockNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var blockPositions = new HashSet<int>();
        var blockPhrases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var fieldIds = new HashSet<Guid>();
        var globalFieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in blocks)
        {
            ValidateBlock(
                block,
                blockIds,
                blockNames,
                blockPositions,
                blockPhrases,
                fieldIds,
                globalFieldNames);
        }
    }

    private static void ValidateBlock(
        TemplateBlockEventDto block,
        HashSet<Guid> blockIds,
        HashSet<string> blockNames,
        HashSet<int> blockPositions,
        Dictionary<string, string> blockPhrases,
        HashSet<Guid> fieldIds,
        HashSet<string> globalFieldNames)
    {
        if (block.Id == Guid.Empty)
            throw new ArgumentException("Block id is required");

        if (!blockIds.Add(block.Id))
            throw new ArgumentException($"Duplicate block id: {block.Id}");

        if (string.IsNullOrWhiteSpace(block.Name))
            throw new ArgumentException("Block name is required");

        var blockName = block.Name.Trim();

        if (blockName.Length > MaxBlockNameLength)
            throw new ArgumentException($"Block name length cannot exceed {MaxBlockNameLength}");

        if (!blockNames.Add(blockName))
            throw new ArgumentException($"Duplicate block name: {blockName}");

        if (block.Position < 0)
            throw new ArgumentException($"Block position cannot be negative: {blockName}");

        if (!blockPositions.Add(block.Position))
            throw new ArgumentException($"Duplicate block position: {block.Position}");

        if (block.Fields is null || block.Fields.Count == 0)
            throw new ArgumentException($"Block fields are required: {blockName}");

        ValidateBlockPhrases(block, blockPhrases);
        ValidateFields(block, fieldIds, globalFieldNames);
        ValidateDefaultField(block);
    }

    private static void ValidateBlockPhrases(
        TemplateBlockEventDto block,
        Dictionary<string, string> blockPhrases)
    {
        if (block.Phrases is null)
            return;

        foreach (var phrase in block.Phrases)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                throw new ArgumentException($"Block phrase cannot be empty: {block.Name}");

            var normalizedPhrase = phrase.Trim();

            if (normalizedPhrase.Length > MaxPhraseLength)
                throw new ArgumentException($"Block phrase length cannot exceed {MaxPhraseLength}: {normalizedPhrase}");

            if (blockPhrases.TryGetValue(normalizedPhrase, out var existingBlock))
            {
                throw new ArgumentException(
                    $"Duplicate block phrase '{normalizedPhrase}' in blocks '{existingBlock}' and '{block.Name}'");
            }

            blockPhrases.Add(normalizedPhrase, block.Name);
        }
    }

    private static void ValidateFields(
        TemplateBlockEventDto block,
        HashSet<Guid> fieldIds,
        HashSet<string> globalFieldNames)
    {
        var fieldPositions = new HashSet<int>();
        var fieldSearchPhrases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in block.Fields)
        {
            ValidateField(
                block,
                field,
                fieldIds,
                globalFieldNames,
                fieldPositions,
                fieldSearchPhrases);
        }
    }

    private static void ValidateField(
        TemplateBlockEventDto block,
        TemplateFieldEventDto field,
        HashSet<Guid> fieldIds,
        HashSet<string> globalFieldNames,
        HashSet<int> fieldPositions,
        Dictionary<string, string> fieldSearchPhrases)
    {
        if (field.Id == Guid.Empty)
            throw new ArgumentException($"Field id is required in block: {block.Name}");

        if (!fieldIds.Add(field.Id))
            throw new ArgumentException($"Duplicate field id: {field.Id}");

        if (string.IsNullOrWhiteSpace(field.FieldName))
            throw new ArgumentException($"FieldName is required in block: {block.Name}");

        var fieldName = field.FieldName.Trim();

        if (fieldName.Length > MaxFieldNameLength)
            throw new ArgumentException($"FieldName length cannot exceed {MaxFieldNameLength}: {fieldName}");

        if (!FieldNameRegex.IsMatch(fieldName))
        {
            throw new ArgumentException(
                $"FieldName has invalid format: {fieldName}. Allowed format: latin letters, numbers and underscore");
        }

        if (!globalFieldNames.Add(fieldName))
            throw new ArgumentException($"Duplicate field name in template: {fieldName}");

        if (string.IsNullOrWhiteSpace(field.DisplayName))
            throw new ArgumentException($"DisplayName is required for field: {fieldName}");

        var displayName = field.DisplayName.Trim();

        if (displayName.Length > MaxDisplayNameLength)
            throw new ArgumentException($"DisplayName length cannot exceed {MaxDisplayNameLength}: {displayName}");

        if (field.Position < 0)
            throw new ArgumentException($"Field position cannot be negative: {fieldName}");

        if (!fieldPositions.Add(field.Position))
            throw new ArgumentException($"Duplicate field position '{field.Position}' in block: {block.Name}");

        if (!Enum.IsDefined(typeof(TemplateFieldType), field.Type))
            throw new ArgumentException($"Invalid field type for field: {fieldName}");

        ValidateFieldSearchPhrases(block, field, fieldSearchPhrases);
        ValidateNorm(field);
    }

    private static void ValidateFieldSearchPhrases(
        TemplateBlockEventDto block,
        TemplateFieldEventDto field,
        Dictionary<string, string> fieldSearchPhrases)
    {
        AddFieldSearchPhrase(
            block.Name,
            field.FieldName,
            field.DisplayName,
            fieldSearchPhrases);

        if (field.Phrases is null)
            return;

        foreach (var phrase in field.Phrases)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                throw new ArgumentException($"Field phrase cannot be empty: {field.FieldName}");

            AddFieldSearchPhrase(
                block.Name,
                field.FieldName,
                phrase,
                fieldSearchPhrases);
        }
    }

    private static void AddFieldSearchPhrase(
        string blockName,
        string fieldName,
        string phrase,
        Dictionary<string, string> fieldSearchPhrases)
    {
        var normalizedPhrase = phrase.Trim();

        if (normalizedPhrase.Length > MaxPhraseLength)
            throw new ArgumentException($"Field phrase length cannot exceed {MaxPhraseLength}: {normalizedPhrase}");

        if (fieldSearchPhrases.TryGetValue(normalizedPhrase, out var existingField))
        {
            throw new ArgumentException(
                $"Duplicate field phrase '{normalizedPhrase}' in block '{blockName}' for fields '{existingField}' and '{fieldName}'");
        }

        fieldSearchPhrases.Add(normalizedPhrase, fieldName);
    }

    private static void ValidateDefaultField(TemplateBlockEventDto block)
    {
        if (string.IsNullOrWhiteSpace(block.DefaultFieldName))
            return;

        var defaultFieldName = block.DefaultFieldName.Trim();

        var exists = block.Fields.Any(field =>
            string.Equals(
                field.FieldName.Trim(),
                defaultFieldName,
                StringComparison.OrdinalIgnoreCase));

        if (!exists)
        {
            throw new ArgumentException(
                $"Default field '{defaultFieldName}' was not found in block: {block.Name}");
        }
    }

    private static void ValidateNorm(TemplateFieldEventDto field)
    {
        if (field.Norm is null)
            return;

        var norm = field.Norm;

        if (norm.Min is not null && norm.Max is not null && norm.Min > norm.Max)
            throw new ArgumentException($"Norm Min cannot be greater than Max for field: {field.FieldName}");

        if (!string.IsNullOrWhiteSpace(norm.Unit) && norm.Unit.Trim().Length > MaxNormUnitLength)
            throw new ArgumentException($"Norm unit length cannot exceed {MaxNormUnitLength}: {field.FieldName}");

        if (!string.IsNullOrWhiteSpace(norm.NormalText) && norm.NormalText.Trim().Length > MaxNormTextLength)
            throw new ArgumentException($"Norm text length cannot exceed {MaxNormTextLength}: {field.FieldName}");

        switch (field.Type)
        {
            case TemplateFieldType.Text:
                ValidateTextNorm(field);
                break;

            case TemplateFieldType.Number:
                ValidateNumberNorm(field);
                break;

            case TemplateFieldType.NumberWithUnit:
                ValidateNumberWithUnitNorm(field);
                break;

            default:
                throw new ArgumentException($"Invalid field type for field: {field.FieldName}");
        }
    }

    private static void ValidateTextNorm(TemplateFieldEventDto field)
    {
        var norm = field.Norm!;

        if (norm.Min is not null || norm.Max is not null)
            throw new ArgumentException($"Text field cannot contain numeric norm: {field.FieldName}");

        if (!string.IsNullOrWhiteSpace(norm.Unit))
            throw new ArgumentException($"Text field cannot contain norm unit: {field.FieldName}");

        if (string.IsNullOrWhiteSpace(norm.NormalText))
            throw new ArgumentException($"Text norm value is required for field: {field.FieldName}");
    }

    private static void ValidateNumberNorm(TemplateFieldEventDto field)
    {
        var norm = field.Norm!;

        if (!string.IsNullOrWhiteSpace(norm.NormalText))
            throw new ArgumentException($"Numeric field cannot contain text norm: {field.FieldName}");
    }

    private static void ValidateNumberWithUnitNorm(TemplateFieldEventDto field)
    {
        var norm = field.Norm!;

        if (!string.IsNullOrWhiteSpace(norm.NormalText))
            throw new ArgumentException($"Numeric field with unit cannot contain text norm: {field.FieldName}");

        if ((norm.Min is not null || norm.Max is not null) && string.IsNullOrWhiteSpace(norm.Unit))
            throw new ArgumentException($"Norm unit is required for field: {field.FieldName}");
    }

    private static readonly Regex FieldNameRegex = new("^[A-Za-z][A-Za-z0-9_]*$", RegexOptions.Compiled);
}