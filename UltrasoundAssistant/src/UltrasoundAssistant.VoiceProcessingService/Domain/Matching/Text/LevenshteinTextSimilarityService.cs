namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Text;

/// <summary>
/// Сервис расчёта похожести текста на основе расстояния Левенштейна
/// </summary>
public sealed class LevenshteinTextSimilarityService : ITextSimilarityService
{
    /// <inheritdoc />
    public double Calculate(string source, string target)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
            return 0;

        source = source.Trim().ToLowerInvariant();
        target = target.Trim().ToLowerInvariant();

        if (source == target)
            return 1;

        if (source.Length >= 4 && target.Length >= 4)
        {
            if (source.Contains(target, StringComparison.OrdinalIgnoreCase) ||
                target.Contains(source, StringComparison.OrdinalIgnoreCase))
            {
                return 0.9;
            }
        }

        var levenshtein = CalculateLevenshtein(source, target);
        var maxLength = Math.Max(source.Length, target.Length);

        if (maxLength == 0)
            return 0;

        var score = 1.0 - (double)levenshtein / maxLength;

        return Math.Clamp(score, 0, 1);
    }

    /// <inheritdoc />
    public double CalculateWords(IReadOnlyList<string> sourceWords, IReadOnlyList<string> targetWords)
    {
        if (sourceWords.Count == 0 || targetWords.Count == 0)
            return 0;

        if (sourceWords.Count != targetWords.Count)
            return 0;

        var total = 0.0;

        for (var i = 0; i < sourceWords.Count; i++)
            total += Calculate(sourceWords[i], targetWords[i]);

        return Math.Clamp(total / sourceWords.Count, 0, 1);
    }

    private static int CalculateLevenshtein(string source, string target)
    {
        var matrix = new int[source.Length + 1, target.Length + 1];

        for (var i = 0; i <= source.Length; i++)
            matrix[i, 0] = i;

        for (var j = 0; j <= target.Length; j++)
            matrix[0, j] = j;

        for (var i = 1; i <= source.Length; i++)
        {
            for (var j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(
                        matrix[i - 1, j] + 1,
                        matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[source.Length, target.Length];
    }
}
