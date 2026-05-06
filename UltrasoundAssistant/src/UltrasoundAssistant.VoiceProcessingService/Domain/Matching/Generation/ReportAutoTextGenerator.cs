using System.Globalization;
using System.Text.RegularExpressions;
using UltrasoundAssistant.Contracts.Entity.Templates;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.Contracts.VoiceProcessing;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Generation;

/// <summary>
/// Генерирует поля описания и заключения по заполненным полям шаблона.
/// </summary>
public sealed class ReportAutoTextGenerator : IReportAutoTextGenerator
{
    private static readonly Regex SpacesRegex = new(@"\s+", RegexOptions.Compiled);

    public void FillMissingAutoFields(VoiceProcessResult result, TemplateDto template)
    {
        if (result.Fields.Count == 0)
            return;

        AddDescriptionIfMissing(result, template);
        AddConclusionIfMissing(result, template);
    }

    private static void AddDescriptionIfMissing(VoiceProcessResult result, TemplateDto template)
    {
        var descriptionField = FindFieldByRole(
            template,
            TemplateFieldRole.Description);

        if (descriptionField is null)
            return;

        if (HasFieldValue(result, descriptionField.Field.FieldName))
            return;

        var description = BuildDescription(result, template);

        if (string.IsNullOrWhiteSpace(description))
            return;

        AddOrReplaceField(
            result,
            new MatchedFieldResult
            {
                BlockName = descriptionField.Block.Name,
                FieldName = descriptionField.Field.FieldName,
                Keyword = "auto.description",
                RecognizedKeyword = "auto.description",
                RawValue = description,
                Value = description,
                Confidence = 1
            });
    }

    private static void AddConclusionIfMissing(VoiceProcessResult result, TemplateDto template)
    {
        var conclusionField = FindFieldByRole(
            template,
            TemplateFieldRole.Conclusion);

        if (conclusionField is null)
            return;

        if (HasFieldValue(result, conclusionField.Field.FieldName))
            return;

        var conclusion = BuildConclusion(result, template);

        if (string.IsNullOrWhiteSpace(conclusion))
            return;

        AddOrReplaceField(
            result,
            new MatchedFieldResult
            {
                BlockName = conclusionField.Block.Name,
                FieldName = conclusionField.Field.FieldName,
                Keyword = "auto.conclusion",
                RecognizedKeyword = "auto.conclusion",
                RawValue = conclusion,
                Value = conclusion,
                Confidence = 1
            });
    }

    private static string BuildDescription(VoiceProcessResult result, TemplateDto template)
    {
        var blockDescriptions = new List<string>();

        foreach (var block in template.Blocks.OrderBy(x => x.Position))
        {
            var fieldDescriptions = new List<string>();

            foreach (var field in block.Fields.OrderBy(x => x.Position))
            {
                if (field.Role != TemplateFieldRole.Regular)
                    continue;

                var matched = FindMatchedField(result, field.FieldName);

                if (matched is null || string.IsNullOrWhiteSpace(matched.Value))
                    continue;

                fieldDescriptions.Add($"{field.DisplayName}: {matched.Value}");
            }

            if (fieldDescriptions.Count == 0)
                continue;

            blockDescriptions.Add($"{block.Name}: {string.Join("; ", fieldDescriptions)}.");
        }

        return string.Join(" ", blockDescriptions);
    }

    private static string BuildConclusion(VoiceProcessResult result, TemplateDto template)
    {
        var deviations = new List<string>();

        foreach (var block in template.Blocks.OrderBy(x => x.Position))
        {
            foreach (var field in block.Fields.OrderBy(x => x.Position))
            {
                if (field.Role != TemplateFieldRole.Regular)
                    continue;

                if (field.Norm is null)
                    continue;

                var matched = FindMatchedField(result, field.FieldName);

                if (matched is null || string.IsNullOrWhiteSpace(matched.Value))
                    continue;

                var deviation = BuildFieldDeviation(field, matched.Value);

                if (!string.IsNullOrWhiteSpace(deviation))
                {
                    deviations.Add($"{field.DisplayName}: {deviation}");
                }
            }
        }

        if (deviations.Count == 0)
            return "Значимых отклонений по заполненным полям не выявлено.";

        return string.Join(". ", deviations.Distinct(StringComparer.OrdinalIgnoreCase)) + ".";
    }

    private static string? BuildFieldDeviation(TemplateFieldDto field, string value)
    {
        if (field.Norm is null)
            return null;

        return field.Type switch
        {
            TemplateFieldType.Number => BuildNumericDeviation(field, value),
            TemplateFieldType.NumberWithUnit => BuildNumericDeviation(field, value),
            TemplateFieldType.Text => BuildTextDeviation(field, value),
            _ => BuildTextDeviation(field, value)
        };
    }

    private static string? BuildNumericDeviation(TemplateFieldDto field, string value)
    {
        var norm = field.Norm;

        if (norm is null)
            return null;

        var numericValue = ExtractDecimal(value);

        if (numericValue is null)
            return null;

        if (norm.Min is not null && numericValue < norm.Min)
        {
            return $"значение ниже нормы ({value}, норма: {FormatNorm(norm)})";
        }

        if (norm.Max is not null && numericValue > norm.Max)
        {
            return $"значение выше нормы ({value}, норма: {FormatNorm(norm)})";
        }

        return null;
    }

    private static string? BuildTextDeviation(TemplateFieldDto field, string value)
    {
        var norm = field.Norm;

        if (norm is null)
            return null;

        if (string.IsNullOrWhiteSpace(norm.NormalText))
            return null;

        if (IsTextValueNormal(value, norm.NormalText))
            return null;

        return $"отклонение от нормы ({value}, норма: {norm.NormalText})";
    }

    private static bool IsTextValueNormal(string value, string normalText)
    {
        var normalizedValue = NormalizeForCompare(value);
        var normalizedNormal = NormalizeForCompare(normalText);

        if (string.IsNullOrWhiteSpace(normalizedValue) ||
            string.IsNullOrWhiteSpace(normalizedNormal))
        {
            return false;
        }

        if (normalizedValue == normalizedNormal)
            return true;

        if (normalizedValue.Contains(normalizedNormal))
            return true;

        if (normalizedNormal.Contains(normalizedValue))
            return true;

        var valueWords = normalizedValue
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var normalWords = normalizedNormal
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (valueWords.Count == 0 || normalWords.Count == 0)
            return false;

        var matchedWords = valueWords.Count(normalWords.Contains);
        var requiredMatches = Math.Min(valueWords.Count, normalWords.Count);

        return matchedWords == requiredMatches;
    }

    private static string NormalizeForCompare(string value)
    {
        var normalized = value
            .ToLowerInvariant()
            .Replace('ё', 'е')
            .Replace(",", " ")
            .Replace(".", " ")
            .Replace(":", " ")
            .Replace(";", " ")
            .Replace("-", " ")
            .Trim();

        return SpacesRegex.Replace(normalized, " ");
    }

    private static decimal? ExtractDecimal(string value)
    {
        var normalized = value
            .ToLowerInvariant()
            .Replace(',', '.')
            .Trim();

        var numberText = new string(
            normalized
                .SkipWhile(x => !char.IsDigit(x) && x != '-' && x != '.')
                .TakeWhile(x => char.IsDigit(x) || x == '-' || x == '.')
                .ToArray());

        if (string.IsNullOrWhiteSpace(numberText))
            return null;

        if (decimal.TryParse(
                numberText,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var number))
        {
            return number;
        }

        return null;
    }

    private static string FormatNorm(FieldNormDto norm)
    {
        if (norm.Min is not null && norm.Max is not null)
            return $"{norm.Min}-{norm.Max}{FormatUnit(norm.Unit)}";

        if (norm.Min is not null)
            return $"от {norm.Min}{FormatUnit(norm.Unit)}";

        if (norm.Max is not null)
            return $"до {norm.Max}{FormatUnit(norm.Unit)}";

        if (!string.IsNullOrWhiteSpace(norm.NormalText))
            return norm.NormalText;

        return "не задана";
    }

    private static string FormatUnit(string? unit)
    {
        return string.IsNullOrWhiteSpace(unit)
            ? string.Empty
            : $" {unit.Trim()}";
    }

    private static MatchedFieldResult? FindMatchedField(VoiceProcessResult result, string fieldName)
    {
        return result.Fields.LastOrDefault(x =>
            string.Equals(
                x.FieldName,
                fieldName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasFieldValue(VoiceProcessResult result, string fieldName)
    {
        return result.Fields.Any(x =>
            string.Equals(
                x.FieldName,
                fieldName,
                StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(x.Value));
    }

    private static FieldWithBlock? FindFieldByRole(TemplateDto template, TemplateFieldRole role)
    {
        foreach (var block in template.Blocks.OrderBy(x => x.Position))
        {
            var field = block.Fields
                .OrderBy(x => x.Position)
                .FirstOrDefault(x => x.Role == role);

            if (field is null)
                continue;

            return new FieldWithBlock
            {
                Block = block,
                Field = field
            };
        }

        return null;
    }

    private static void AddOrReplaceField(VoiceProcessResult result, MatchedFieldResult field)
    {
        var existingIndex = result.Fields.FindLastIndex(x =>
            string.Equals(
                x.FieldName,
                field.FieldName,
                StringComparison.OrdinalIgnoreCase));

        if (existingIndex >= 0)
        {
            result.Fields[existingIndex] = field;
            return;
        }

        result.Fields.Add(field);
    }

    private sealed class FieldWithBlock
    {
        public TemplateBlockDto Block { get; init; } = null!;

        public TemplateFieldDto Field { get; init; } = null!;
    }
}
