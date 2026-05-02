using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Values;

/// <summary>
/// Сервис извлечения значения поля из текста
/// </summary>
public sealed class ValueExtractor : IValueExtractor
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "далее",
        "затем",
        "следующий",
        "следующая",
        "следующее"
    };

    /// <inheritdoc />
    public string Extract(IReadOnlyList<TextToken> tokens, int startIndex, int endIndex, int maxWords)
    {
        if (tokens.Count == 0)
            return string.Empty;

        if (startIndex < 0)
            startIndex = 0;

        if (endIndex > tokens.Count)
            endIndex = tokens.Count;

        if (startIndex >= endIndex)
            return string.Empty;

        var result = new List<string>();

        for (var i = startIndex; i < endIndex && result.Count < maxWords; i++)
        {
            var token = tokens[i];

            if (StopWords.Contains(token.Normalized))
                break;

            result.Add(token.Original);
        }

        return string.Join(' ', result).Trim();
    }
}
