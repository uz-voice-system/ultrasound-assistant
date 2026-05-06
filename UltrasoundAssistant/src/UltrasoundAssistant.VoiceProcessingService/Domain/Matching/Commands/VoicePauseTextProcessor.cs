using System.Text.RegularExpressions;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Commands;

/// <summary>
/// Удаляет из распознанного текста фрагменты между словами "пауза" и "продолжить".
/// </summary>
public sealed class VoicePauseTextProcessor
{
    private static readonly Regex SpacesRegex = new(@"\s+", RegexOptions.Compiled);

    private static readonly string[] PauseWords =
    [
        "пауза",
        "приостановить"
    ];

    private static readonly string[] ResumeWords =
    [
        "продолжить",
        "возобновить",
        "дальше"
    ];

    public VoicePauseProcessingResult Process(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new VoicePauseProcessingResult
            {
                ProcessedText = string.Empty
            };
        }

        var words = SplitWords(text);

        var resultWords = new List<string>();
        var ignoredWords = new List<string>();

        var paused = false;
        var pauseFound = false;
        var resumeFound = false;

        foreach (var word in words)
        {
            if (PauseWords.Contains(word, StringComparer.OrdinalIgnoreCase))
            {
                paused = true;
                pauseFound = true;
                continue;
            }

            if (ResumeWords.Contains(word, StringComparer.OrdinalIgnoreCase))
            {
                paused = false;
                resumeFound = true;
                continue;
            }

            if (paused)
            {
                ignoredWords.Add(word);
                continue;
            }

            resultWords.Add(word);
        }

        return new VoicePauseProcessingResult
        {
            ProcessedText = string.Join(' ', resultWords),
            IgnoredText = string.Join(' ', ignoredWords),
            PauseFound = pauseFound,
            ResumeFound = resumeFound,
            IsPausedAtEnd = paused
        };
    }

    private static IReadOnlyList<string> SplitWords(string text)
    {
        var normalized = text
            .ToLowerInvariant()
            .Replace('ё', 'е')
            .Replace(",", " ")
            .Replace(".", " ")
            .Replace(":", " ")
            .Replace(";", " ")
            .Replace("-", " ");

        normalized = SpacesRegex.Replace(normalized, " ").Trim();

        if (string.IsNullOrWhiteSpace(normalized))
            return [];

        return normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }
}
