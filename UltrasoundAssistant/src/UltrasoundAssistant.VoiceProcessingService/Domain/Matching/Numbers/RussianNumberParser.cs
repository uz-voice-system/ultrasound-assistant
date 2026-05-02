using System.Globalization;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Text;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Numbers;

/// <summary>
/// Парсер русских числительных
/// </summary>
public sealed class RussianNumberParser : INumberParser
{
    private const double MinimumNumberWordConfidence = 0.78;

    private readonly ITextSimilarityService _similarityService;

    private static readonly Dictionary<string, int> Units = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ноль"] = 0,
        ["один"] = 1,
        ["одна"] = 1,
        ["одно"] = 1,
        ["два"] = 2,
        ["две"] = 2,
        ["три"] = 3,
        ["четыре"] = 4,
        ["пять"] = 5,
        ["шесть"] = 6,
        ["семь"] = 7,
        ["восемь"] = 8,
        ["девять"] = 9
    };

    private static readonly Dictionary<string, int> Teens = new(StringComparer.OrdinalIgnoreCase)
    {
        ["десять"] = 10,
        ["одиннадцать"] = 11,
        ["двенадцать"] = 12,
        ["тринадцать"] = 13,
        ["четырнадцать"] = 14,
        ["пятнадцать"] = 15,
        ["шестнадцать"] = 16,
        ["семнадцать"] = 17,
        ["восемнадцать"] = 18,
        ["девятнадцать"] = 19
    };

    private static readonly Dictionary<string, int> Tens = new(StringComparer.OrdinalIgnoreCase)
    {
        ["двадцать"] = 20,
        ["тридцать"] = 30,
        ["сорок"] = 40,
        ["пятьдесят"] = 50,
        ["шестьдесят"] = 60,
        ["семьдесят"] = 70,
        ["восемьдесят"] = 80,
        ["девяносто"] = 90
    };

    private static readonly Dictionary<string, int> Hundreds = new(StringComparer.OrdinalIgnoreCase)
    {
        ["сто"] = 100,
        ["двести"] = 200,
        ["триста"] = 300,
        ["четыреста"] = 400,
        ["пятьсот"] = 500,
        ["шестьсот"] = 600,
        ["семьсот"] = 700,
        ["восемьсот"] = 800,
        ["девятьсот"] = 900
    };

    private static readonly Dictionary<string, decimal> FractionMultipliers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["десятая"] = 0.1m,
        ["десятых"] = 0.1m,
        ["десятой"] = 0.1m,
        ["сотая"] = 0.01m,
        ["сотых"] = 0.01m,
        ["сотой"] = 0.01m
    };

    public RussianNumberParser(ITextSimilarityService similarityService)
    {
        _similarityService = similarityService;
    }

    /// <inheritdoc />
    public NumberParseResult Parse(IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0)
            return NumberParseResult.Failed();

        var direct = TryParseDirectNumber(tokens[0]);

        if (direct is not null)
        {
            return new NumberParseResult
            {
                Success = true,
                Value = direct.Value,
                UsedTokens = 1,
                Confidence = 1
            };
        }

        if (TryParseHalf(tokens, out var halfResult))
            return halfResult;

        var integer = ParseInteger(tokens, 0);

        if (!integer.Success)
            return NumberParseResult.Failed();

        var value = integer.Value;
        var usedTokens = integer.UsedTokens;
        var confidence = integer.Confidence;

        if (tokens.Count > usedTokens && IsDecimalSeparator(tokens[usedTokens]))
        {
            var fraction = ParseInteger(tokens, usedTokens + 1);

            if (fraction.Success)
            {
                var fractionWordIndex = usedTokens + 1 + fraction.UsedTokens;

                if (fractionWordIndex < tokens.Count &&
                    TryGetFractionMultiplier(tokens[fractionWordIndex], out var multiplier, out var fractionWordConfidence))
                {
                    value += fraction.Value * multiplier;
                    usedTokens = fractionWordIndex + 1;
                    confidence = Math.Min(confidence, Math.Min(fraction.Confidence, fractionWordConfidence));
                }
            }
        }
        else if (tokens.Count > usedTokens &&
                 TryGetFractionMultiplier(tokens[usedTokens], out var multiplier, out var fractionOnlyConfidence) &&
                 integer.Value < 10)
        {
            value = integer.Value * multiplier;
            usedTokens += 1;
            confidence = Math.Min(confidence, fractionOnlyConfidence);
        }

        return new NumberParseResult
        {
            Success = true,
            Value = value,
            UsedTokens = usedTokens,
            Confidence = Math.Round(confidence, 2)
        };
    }

    private NumberParseResult ParseInteger(IReadOnlyList<string> tokens, int startIndex)
    {
        var total = 0;
        var used = 0;
        var confidence = 1.0;

        for (var i = startIndex; i < tokens.Count; i++)
        {
            var token = tokens[i];

            if (TryGetNumberWord(token, Hundreds, out var hundred, out var hundredConfidence))
            {
                total += hundred;
                used++;
                confidence = Math.Min(confidence, hundredConfidence);
                continue;
            }

            if (TryGetNumberWord(token, Tens, out var ten, out var tenConfidence))
            {
                total += ten;
                used++;
                confidence = Math.Min(confidence, tenConfidence);
                continue;
            }

            if (TryGetNumberWord(token, Teens, out var teen, out var teenConfidence))
            {
                total += teen;
                used++;
                confidence = Math.Min(confidence, teenConfidence);
                continue;
            }

            if (TryGetNumberWord(token, Units, out var unit, out var unitConfidence))
            {
                total += unit;
                used++;
                confidence = Math.Min(confidence, unitConfidence);
                continue;
            }

            break;
        }

        if (used == 0)
            return NumberParseResult.Failed();

        return new NumberParseResult
        {
            Success = true,
            Value = total,
            UsedTokens = used,
            Confidence = Math.Round(confidence, 2)
        };
    }

    private decimal? TryParseDirectNumber(string token)
    {
        token = token.Replace(',', '.');

        if (decimal.TryParse(
                token,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var value))
        {
            return value;
        }

        return null;
    }

    private bool TryGetNumberWord(
        string token,
        IReadOnlyDictionary<string, int> dictionary,
        out int value,
        out double confidence)
    {
        if (dictionary.TryGetValue(token, out value))
        {
            confidence = 1;
            return true;
        }

        var best = dictionary
            .Select(x => new
            {
                Word = x.Key,
                Value = x.Value,
                Score = _similarityService.Calculate(token, x.Key)
            })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        if (best is not null && best.Score >= MinimumNumberWordConfidence)
        {
            value = best.Value;
            confidence = best.Score;
            return true;
        }

        value = 0;
        confidence = 0;
        return false;
    }

    private bool TryGetFractionMultiplier(
        string token,
        out decimal multiplier,
        out double confidence)
    {
        if (FractionMultipliers.TryGetValue(token, out multiplier))
        {
            confidence = 1;
            return true;
        }

        var best = FractionMultipliers
            .Select(x => new
            {
                Word = x.Key,
                Value = x.Value,
                Score = _similarityService.Calculate(token, x.Key)
            })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        if (best is not null && best.Score >= MinimumNumberWordConfidence)
        {
            multiplier = best.Value;
            confidence = best.Score;
            return true;
        }

        multiplier = 0;
        confidence = 0;
        return false;
    }

    private static bool TryParseHalf(
        IReadOnlyList<string> tokens,
        out NumberParseResult result)
    {
        if (tokens[0] is "полтора" or "полторы")
        {
            result = new NumberParseResult
            {
                Success = true,
                Value = 1.5m,
                UsedTokens = 1,
                Confidence = 1
            };

            return true;
        }

        result = NumberParseResult.Failed();
        return false;
    }

    private static bool IsDecimalSeparator(string token)
    {
        return token is "целая" or "целых" or "целое" or "точка";
    }
}
