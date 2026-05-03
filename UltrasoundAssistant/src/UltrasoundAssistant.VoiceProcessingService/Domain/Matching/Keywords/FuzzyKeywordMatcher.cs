using UltrasoundAssistant.Contracts.Entity.Templates;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Text;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Keywords;

/// <summary>
/// Сервис нечёткого поиска ключевых слов
/// </summary>
public sealed class FuzzyKeywordMatcher : IKeywordMatcher
{
    private const double MinimumBlockConfidence = 0.72;
    private const double MinimumFieldConfidence = 0.70;

    private readonly ITextNormalizer _textNormalizer;
    private readonly ITextSimilarityService _similarityService;

    public FuzzyKeywordMatcher(
        ITextNormalizer textNormalizer,
        ITextSimilarityService similarityService)
    {
        _textNormalizer = textNormalizer;
        _similarityService = similarityService;
    }

    /// <inheritdoc />
    public KeywordOccurrence? FindBestBlockMatch(
        IReadOnlyList<TextToken> tokens,
        TemplateDto template,
        int startIndex)
    {
        if (startIndex < 0 || startIndex >= tokens.Count)
            return null;

        KeywordOccurrence? best = null;

        foreach (var block in template.Blocks)
        {
            foreach (var phrase in GetBlockPhrases(block))
            {
                var candidate = CreateBlockCandidate(block, phrase);

                var occurrence = TryCreateOccurrence(
                    tokens,
                    candidate,
                    startIndex,
                    MinimumBlockConfidence);

                if (occurrence is null)
                    continue;

                if (IsBetter(occurrence, best))
                    best = occurrence;
            }
        }

        return best;
    }

    /// <inheritdoc />
    public KeywordOccurrence? FindBestFieldMatch(
        IReadOnlyList<TextToken> tokens,
        TemplateBlockDto block,
        int startIndex)
    {
        if (startIndex < 0 || startIndex >= tokens.Count)
            return null;

        KeywordOccurrence? best = null;

        foreach (var field in block.Fields)
        {
            foreach (var phrase in GetFieldPhrases(field))
            {
                var candidate = CreateFieldCandidate(block, field, phrase);

                var occurrence = TryCreateOccurrence(
                    tokens,
                    candidate,
                    startIndex,
                    MinimumFieldConfidence);

                if (occurrence is null)
                    continue;

                if (IsBetter(occurrence, best))
                    best = occurrence;
            }
        }

        return best;
    }

    private KeywordOccurrence? TryCreateOccurrence(
        IReadOnlyList<TextToken> tokens,
        KeywordMatchCandidate candidate,
        int startIndex,
        double minimumConfidence)
    {
        if (candidate.NormalizedWords.Count == 0)
            return null;

        if (startIndex + candidate.NormalizedWords.Count > tokens.Count)
            return null;

        var sourceWords = tokens
            .Skip(startIndex)
            .Take(candidate.NormalizedWords.Count)
            .Select(x => x.Normalized)
            .ToArray();

        var confidence = _similarityService.CalculateWords(
            sourceWords,
            candidate.NormalizedWords);

        if (confidence < minimumConfidence)
            return null;

        var recognizedKeyword = string.Join(' ',
            tokens
                .Skip(startIndex)
                .Take(candidate.NormalizedWords.Count)
                .Select(x => x.Original));

        return new KeywordOccurrence
        {
            Kind = candidate.Kind,
            WordIndex = startIndex,
            WordCount = candidate.NormalizedWords.Count,
            BlockId = candidate.BlockId,
            BlockName = candidate.BlockName,
            FieldName = candidate.FieldName,
            Keyword = candidate.Phrase,
            RecognizedKeyword = recognizedKeyword,
            Confidence = Math.Round(confidence, 2)
        };
    }

    private KeywordMatchCandidate CreateBlockCandidate(TemplateBlockDto block, string phrase)
    {
        var normalizedWords = _textNormalizer
            .Tokenize(phrase)
            .Select(x => x.Normalized)
            .ToArray();

        return new KeywordMatchCandidate
        {
            Kind = KeywordMatchKind.Block,
            BlockId = block.Id,
            BlockName = block.Name,
            FieldName = null,
            Phrase = phrase,
            NormalizedWords = normalizedWords
        };
    }

    private KeywordMatchCandidate CreateFieldCandidate(
        TemplateBlockDto block,
        TemplateFieldDto field,
        string phrase)
    {
        var normalizedWords = _textNormalizer
            .Tokenize(phrase)
            .Select(x => x.Normalized)
            .ToArray();

        return new KeywordMatchCandidate
        {
            Kind = KeywordMatchKind.Field,
            BlockId = block.Id,
            BlockName = block.Name,
            FieldName = field.FieldName,
            Phrase = phrase,
            NormalizedWords = normalizedWords
        };
    }

    private static IEnumerable<string> GetBlockPhrases(TemplateBlockDto block)
    {
        if (!string.IsNullOrWhiteSpace(block.Name))
            yield return block.Name;

        foreach (var phrase in block.Phrases.Where(x => !string.IsNullOrWhiteSpace(x)))
            yield return phrase;
    }

    private static IEnumerable<string> GetFieldPhrases(TemplateFieldDto field)
    {
        if (!string.IsNullOrWhiteSpace(field.DisplayName))
            yield return field.DisplayName;

        foreach (var phrase in field.Phrases.Where(x => !string.IsNullOrWhiteSpace(x)))
            yield return phrase;
    }

    private static bool IsBetter(KeywordOccurrence occurrence, KeywordOccurrence? currentBest)
    {
        if (currentBest is null)
            return true;

        if (occurrence.Confidence > currentBest.Confidence)
            return true;

        if (Math.Abs(occurrence.Confidence - currentBest.Confidence) < 0.001 &&
            occurrence.WordCount > currentBest.WordCount)
        {
            return true;
        }

        return false;
    }
}
