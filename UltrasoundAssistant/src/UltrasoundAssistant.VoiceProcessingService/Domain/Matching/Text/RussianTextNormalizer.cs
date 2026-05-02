using System.Text.RegularExpressions;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Text;

/// <summary>
/// Нормализатор русского текста
/// </summary>
public sealed class RussianTextNormalizer : ITextNormalizer
{
    private static readonly Regex CleanupRegex = new(@"[^\p{L}\p{N}\.\,/%\-\s]", RegexOptions.Compiled);
    private static readonly Regex SpacesRegex = new(@"\s+", RegexOptions.Compiled);

    private static readonly string[] Endings =
    [
        "иями", "ями", "ами",
        "его", "ого", "ему", "ому",
        "ыми", "ими",
        "иях", "ах", "ях",
        "ией", "ею", "ою",
        "ого", "ему",
        "ов", "ев", "ей",
        "ом", "ем", "ой", "ый", "ий",
        "ая", "яя", "ое", "ее",
        "ые", "ие",
        "ам", "ям",
        "ою", "ею",
        "а", "я", "ы", "и", "у", "ю", "е", "о"
    ];

    private static readonly Dictionary<string, string> WordOverrides = new(StringComparer.OrdinalIgnoreCase)
    {
        ["левый"] = "лев",
        ["левая"] = "лев",
        ["левое"] = "лев",
        ["левой"] = "лев",
        ["левую"] = "лев",

        ["правый"] = "прав",
        ["правая"] = "прав",
        ["правое"] = "прав",
        ["правой"] = "прав",
        ["правую"] = "прав",

        ["почка"] = "почк",
        ["почки"] = "почк",
        ["почку"] = "почк",
        ["почкой"] = "почк",

        ["лоханка"] = "лоханк",
        ["лоханки"] = "лоханк",
        ["лоханку"] = "лоханк",

        ["длина"] = "длин",
        ["длинна"] = "длин",
        ["длино"] = "длин",

        ["ширина"] = "ширин",
        ["ширинна"] = "ширин",
        ["шириннна"] = "ширин",

        ["толщина"] = "толщин",
        ["толщена"] = "толщин",

        ["сантиметр"] = "сантиметр",
        ["сантиметра"] = "сантиметр",
        ["сантиметров"] = "сантиметр",
        ["санметров"] = "сантиметр",
        ["санметр"] = "сантиметр",

        ["миллиметр"] = "миллиметр",
        ["миллиметра"] = "миллиметр",
        ["миллиметров"] = "миллиметр",
        ["милиметров"] = "миллиметр",
        ["милиметр"] = "миллиметр",

        ["изменений"] = "изменен",
        ["изменения"] = "изменен",
        ["изменен"] = "изменен"
    };

    /// <inheritdoc />
    public string NormalizeText(string text)
    {
        return string.Join(' ', Tokenize(text).Select(x => x.Normalized));
    }

    /// <inheritdoc />
    public string NormalizeWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return string.Empty;

        var value = word
            .ToLowerInvariant()
            .Replace('ё', 'е')
            .Replace(',', '.')
            .Trim('.', ',', ':', ';', '-', ' ', '\t', '\r', '\n');

        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        if (WordOverrides.TryGetValue(value, out var overridden))
            return overridden;

        if (decimal.TryParse(
                value,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out _))
        {
            return value;
        }

        if (value.Length <= 4)
            return value;

        foreach (var ending in Endings.OrderByDescending(x => x.Length))
        {
            if (!value.EndsWith(ending, StringComparison.OrdinalIgnoreCase))
                continue;

            var stemLength = value.Length - ending.Length;

            if (stemLength < 4)
                continue;

            var stem = value[..stemLength];

            if (WordOverrides.TryGetValue(stem, out var stemOverride))
                return stemOverride;

            return stem;
        }

        return value;
    }

    /// <inheritdoc />
    public IReadOnlyList<TextToken> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var cleaned = text
            .ToLowerInvariant()
            .Replace('ё', 'е');

        cleaned = CleanupRegex.Replace(cleaned, " ");
        cleaned = SpacesRegex.Replace(cleaned, " ").Trim();

        if (string.IsNullOrWhiteSpace(cleaned))
            return [];

        var sourceTokens = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new List<TextToken>();

        for (var i = 0; i < sourceTokens.Length; i++)
        {
            var original = sourceTokens[i].Trim('.', ',', ':', ';', '-', ' ');

            if (string.IsNullOrWhiteSpace(original))
                continue;

            var normalized = NormalizeWord(original);

            if (string.IsNullOrWhiteSpace(normalized))
                continue;

            result.Add(new TextToken
            {
                Index = result.Count,
                Original = original,
                Normalized = normalized
            });
        }

        return result;
    }
}
