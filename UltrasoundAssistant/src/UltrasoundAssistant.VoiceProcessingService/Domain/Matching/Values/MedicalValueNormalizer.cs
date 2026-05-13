using System.Globalization;
using UltrasoundAssistant.Contracts.Entity.Templates;
using UltrasoundAssistant.Contracts.VoiceProcessing;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Numbers;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Text;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Values;

/// <summary>
/// Нормализатор медицинских значений полей отчёта
/// </summary>
public sealed class MedicalValueNormalizer : IValueNormalizer
{
    private const double MinimumUnitConfidence = 0.72;
    private const double MinimumTextNormConfidence = 0.72;

    private readonly INumberParser _numberParser;
    private readonly ITextNormalizer _textNormalizer;
    private readonly ITextSimilarityService _similarityService;

    private static readonly Dictionary<string, string> Units = new(StringComparer.OrdinalIgnoreCase)
    {
        ["мм"] = "мм",
        ["миллиметр"] = "мм",

        ["см"] = "см",
        ["сантиметр"] = "см",

        ["процент"] = "%",
        ["%"] = "%",

        ["удар"] = "уд/мин",
        ["уд"] = "уд/мин",
        ["уд/мин"] = "уд/мин",

        ["мин"] = "мин",
        ["минута"] = "мин"
    };

    public MedicalValueNormalizer(
        INumberParser numberParser,
        ITextNormalizer textNormalizer,
        ITextSimilarityService similarityService)
    {
        _numberParser = numberParser;
        _textNormalizer = textNormalizer;
        _similarityService = similarityService;
    }

    /// <inheritdoc />
    public NormalizedValueResult Normalize(string rawValue, TemplateFieldDto field)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return NormalizedValueResult.Failed();

        return field.Type switch
        {
            TemplateFieldType.Number => NormalizeNumber(rawValue, field),
            TemplateFieldType.NumberWithUnit => NormalizeNumberWithUnit(rawValue, field),
            _ => NormalizeText(rawValue, field)
        };
    }

    private NormalizedValueResult NormalizeNumber(string rawValue, TemplateFieldDto field)
    {
        var tokens = GetNormalizedTokens(rawValue);
        var parsedNumber = _numberParser.Parse(tokens);

        if (!parsedNumber.Success)
        {
            return new NormalizedValueResult
            {
                Value = rawValue.Trim(),
                NumericValue = null,
                Unit = null,
                Confidence = 0.45,
                NormStatus = NormStatus.Unknown,
                NormMessage = "Не удалось определить числовое значение"
            };
        }

        var displayValue = FormatDecimal(parsedNumber.Value);
        var norm = CheckNumericNorm(parsedNumber.Value, null, field.Norm);

        return new NormalizedValueResult
        {
            Value = displayValue,
            NumericValue = parsedNumber.Value,
            Unit = null,
            Confidence = parsedNumber.Confidence,
            NormStatus = norm.Status,
            NormMessage = norm.Message
        };
    }

    private NormalizedValueResult NormalizeNumberWithUnit(string rawValue, TemplateFieldDto field)
    {
        var tokens = GetNormalizedTokens(rawValue);
        var parsedNumber = _numberParser.Parse(tokens);

        if (!parsedNumber.Success)
        {
            return new NormalizedValueResult
            {
                Value = rawValue.Trim(),
                NumericValue = null,
                Unit = null,
                Confidence = 0.45,
                NormStatus = NormStatus.Unknown,
                NormMessage = "Не удалось определить числовое значение"
            };
        }

        var unit = FindUnit(
            tokens.Skip(parsedNumber.UsedTokens).ToArray(),
            field.Norm?.Unit,
            out var unitConfidence);

        var valueForNorm = parsedNumber.Value;
        var unitForNorm = unit;

        if (!string.IsNullOrWhiteSpace(field.Norm?.Unit) &&
            !string.IsNullOrWhiteSpace(unit))
        {
            valueForNorm = ConvertUnit(parsedNumber.Value, unit, field.Norm.Unit, out unitForNorm);
        }

        var displayValue = FormatDecimal(parsedNumber.Value);

        if (!string.IsNullOrWhiteSpace(unit))
            displayValue += $" {unit}";

        var norm = CheckNumericNorm(valueForNorm, unitForNorm, field.Norm);

        return new NormalizedValueResult
        {
            Value = displayValue,
            NumericValue = parsedNumber.Value,
            Unit = unit,
            Confidence = Math.Round(parsedNumber.Confidence * unitConfidence, 2),
            NormStatus = norm.Status,
            NormMessage = norm.Message
        };
    }

    private NormalizedValueResult NormalizeText(string rawValue, TemplateFieldDto field)
    {
        var displayValue = rawValue.Trim();
        var normalizedValue = _textNormalizer.NormalizeText(displayValue);

        if (string.IsNullOrWhiteSpace(normalizedValue))
            return NormalizedValueResult.Failed();

        if (string.IsNullOrWhiteSpace(field.Norm?.NormalText))
        {
            return new NormalizedValueResult
            {
                Value = displayValue,
                NumericValue = null,
                Unit = null,
                Confidence = 0.9,
                NormStatus = NormStatus.Unknown
            };
        }

        var normalizedNorm = _textNormalizer.NormalizeText(field.Norm.NormalText);
        var normSimilarity = _similarityService.Calculate(normalizedValue, normalizedNorm);

        if (normSimilarity >= MinimumTextNormConfidence)
        {
            return new NormalizedValueResult
            {
                Value = field.Norm.NormalText.Trim(),
                NumericValue = null,
                Unit = null,
                Confidence = 1,
                NormStatus = NormStatus.Normal,
                NormMessage = "Текстовое значение соответствует норме"
            };
        }

        return new NormalizedValueResult
        {
            Value = displayValue,
            NumericValue = null,
            Unit = null,
            Confidence = 1,
            NormStatus = NormStatus.AbnormalText,
            NormMessage = "Текстовое значение отличается от нормы"
        };
    }

    private string? FindUnit(IReadOnlyList<string> tokens, string? expectedUnit, out double confidence)
    {
        confidence = 0.85;

        foreach (var token in tokens)
        {
            if (Units.TryGetValue(token, out var direct))
            {
                confidence = 1;
                return direct;
            }

            var best = Units
                .Select(x => new
                {
                    Unit = x.Value,
                    Score = _similarityService.Calculate(token, x.Key)
                })
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            if (best is not null && best.Score >= MinimumUnitConfidence)
            {
                confidence = best.Score;
                return best.Unit;
            }
        }

        if (!string.IsNullOrWhiteSpace(expectedUnit))
        {
            confidence = 0.8;
            return NormalizeUnit(expectedUnit);
        }

        confidence = 0.65;
        return null;
    }

    private IReadOnlyList<string> GetNormalizedTokens(string rawValue)
    {
        return _textNormalizer
            .Tokenize(rawValue)
            .Select(x => x.Normalized)
            .ToArray();
    }

    private static string? NormalizeUnit(string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
            return null;

        return Units.TryGetValue(unit.Trim().ToLowerInvariant(), out var normalized)
            ? normalized
            : unit.Trim();
    }

    private static decimal ConvertUnit(decimal value, string sourceUnit, string targetUnit, out string normalizedTargetUnit)
    {
        sourceUnit = NormalizeUnit(sourceUnit) ?? sourceUnit;
        targetUnit = NormalizeUnit(targetUnit) ?? targetUnit;
        normalizedTargetUnit = targetUnit;

        if (sourceUnit == targetUnit)
            return value;

        if (sourceUnit == "мм" && targetUnit == "см")
            return value / 10m;

        if (sourceUnit == "см" && targetUnit == "мм")
            return value * 10m;

        return value;
    }

    private static (NormStatus Status, string? Message) CheckNumericNorm(decimal value, string? unit, FieldNormDto? norm)
    {
        if (norm is null || (norm.Min is null && norm.Max is null))
            return (NormStatus.Unknown, null);

        if (norm.Min is not null && value < norm.Min.Value)
        {
            var message = string.IsNullOrWhiteSpace(norm.Unit)
                ? $"Значение ниже нормы: {FormatDecimal(norm.Min.Value)}"
                : $"Значение ниже нормы: {FormatDecimal(norm.Min.Value)} {norm.Unit}";

            return (NormStatus.Below, message);
        }

        if (norm.Max is not null && value > norm.Max.Value)
        {
            var message = string.IsNullOrWhiteSpace(norm.Unit)
                ? $"Значение выше нормы: {FormatDecimal(norm.Max.Value)}"
                : $"Значение выше нормы: {FormatDecimal(norm.Max.Value)} {norm.Unit}";

            return (NormStatus.Above, message);
        }

        return (NormStatus.Normal, "Значение соответствует норме");
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
