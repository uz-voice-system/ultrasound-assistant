using UltrasoundAssistant.Contracts.Entity.Templates;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Keywords;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Text;

namespace UltrasoundAssistant.Tests.TestDoubles;

public sealed class TestTextSimilarityService : ITextSimilarityService
{
    public double Calculate(string source, string target)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
            return 0;

        source = source.Trim().ToLowerInvariant();
        target = target.Trim().ToLowerInvariant();

        if (source == target)
            return 1;

        if (source.Contains(target) || target.Contains(source))
            return 0.85;

        var distance = CalculateLevenshteinDistance(source, target);
        var maxLength = Math.Max(source.Length, target.Length);

        if (maxLength == 0)
            return 1;

        return Math.Max(0, 1.0 - (double)distance / maxLength);
    }

    public double CalculateWords(IReadOnlyList<string> sourceWords, IReadOnlyList<string> targetWords)
    {
        if (sourceWords.Count == 0 || targetWords.Count == 0)
            return 0;

        var source = string.Join(' ', sourceWords);
        var target = string.Join(' ', targetWords);

        return Calculate(source, target);
    }

    private static int CalculateLevenshteinDistance(string source, string target)
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

public sealed class TestKeywordMatcher : IKeywordMatcher
{
    private readonly ITextNormalizer _textNormalizer;

    public TestKeywordMatcher(ITextNormalizer textNormalizer)
    {
        _textNormalizer = textNormalizer;
    }

    public KeywordOccurrence? FindBestBlockMatch(
        IReadOnlyList<TextToken> tokens,
        TemplateDto template,
        int startIndex)
    {
        foreach (var block in template.Blocks.OrderBy(x => x.Position))
        {
            foreach (var phrase in block.Phrases)
            {
                if (!TryMatchPhrase(tokens, startIndex, phrase, out var wordCount))
                    continue;

                return new KeywordOccurrence
                {
                    Kind = KeywordMatchKind.Block,
                    BlockId = block.Id,
                    FieldName = null,
                    Keyword = phrase,
                    RecognizedKeyword = BuildRecognizedKeyword(tokens, startIndex, wordCount),
                    WordIndex = startIndex,
                    WordCount = wordCount,
                    Confidence = 1
                };
            }
        }

        return null;
    }

    public KeywordOccurrence? FindBestFieldMatch(
        IReadOnlyList<TextToken> tokens,
        TemplateBlockDto block,
        int startIndex)
    {
        foreach (var field in block.Fields.OrderBy(x => x.Position))
        {
            var phrases = new List<string>();

            if (!string.IsNullOrWhiteSpace(field.DisplayName))
                phrases.Add(field.DisplayName);

            if (!string.IsNullOrWhiteSpace(field.FieldName))
                phrases.Add(field.FieldName);

            phrases.AddRange(field.Phrases);

            foreach (var phrase in phrases)
            {
                if (!TryMatchPhrase(tokens, startIndex, phrase, out var wordCount))
                    continue;

                return new KeywordOccurrence
                {
                    Kind = KeywordMatchKind.Field,
                    BlockId = block.Id,
                    FieldName = field.FieldName,
                    Keyword = phrase,
                    RecognizedKeyword = BuildRecognizedKeyword(tokens, startIndex, wordCount),
                    WordIndex = startIndex,
                    WordCount = wordCount,
                    Confidence = 1
                };
            }
        }

        return null;
    }

    private bool TryMatchPhrase(
        IReadOnlyList<TextToken> tokens,
        int startIndex,
        string phrase,
        out int wordCount)
    {
        wordCount = 0;

        if (string.IsNullOrWhiteSpace(phrase))
            return false;

        var phraseTokens = _textNormalizer.Tokenize(phrase);

        if (phraseTokens.Count == 0)
            return false;

        if (startIndex + phraseTokens.Count > tokens.Count)
            return false;

        for (var i = 0; i < phraseTokens.Count; i++)
        {
            var source = tokens[startIndex + i].Normalized;
            var target = phraseTokens[i].Normalized;

            if (!string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        wordCount = phraseTokens.Count;
        return true;
    }

    private static string BuildRecognizedKeyword(
        IReadOnlyList<TextToken> tokens,
        int startIndex,
        int wordCount)
    {
        return string.Join(' ',
            tokens
                .Skip(startIndex)
                .Take(wordCount)
                .Select(x => x.Original));
    }
}
