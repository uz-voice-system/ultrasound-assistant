using UltrasoundAssistant.Contracts.Entity.Templates;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.Contracts.VoiceProcessing;
using UltrasoundAssistant.VoiceProcessingService.Application.Abstractions;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Commands;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Generation;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Keywords;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Text;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Values;

namespace UltrasoundAssistant.VoiceProcessingService.Services.Templates;

/// <summary>
/// Сервис сопоставления распознанного текста с шаблоном отчёта
/// </summary>
public sealed class TemplateMatchingService : ITemplateMatchingService
{
    private const int MaxNumericValueWords = 6;
    private const int MaxTextValueWords = 8;

    private readonly ITextNormalizer _textNormalizer;
    private readonly IKeywordMatcher _keywordMatcher;
    private readonly IValueExtractor _valueExtractor;
    private readonly IValueNormalizer _valueNormalizer;
    private readonly IReportAutoTextGenerator _autoTextGenerator;
    private readonly VoicePauseTextProcessor _pauseTextProcessor;

    public TemplateMatchingService(
        ITextNormalizer textNormalizer,
        IKeywordMatcher keywordMatcher,
        IValueExtractor valueExtractor,
        IValueNormalizer valueNormalizer,
        IReportAutoTextGenerator autoTextGenerator,
        VoicePauseTextProcessor pauseTextProcessor)
    {
        _textNormalizer = textNormalizer;
        _keywordMatcher = keywordMatcher;
        _valueExtractor = valueExtractor;
        _valueNormalizer = valueNormalizer;
        _autoTextGenerator = autoTextGenerator;
        _pauseTextProcessor = pauseTextProcessor;
    }

    /// <inheritdoc />
    public VoiceProcessResult Match(string recognizedText, TemplateDto template)
    {
        if (string.IsNullOrWhiteSpace(recognizedText))
        {
            return new VoiceProcessResult
            {
                Matched = false,
                Error = "Recognized text is empty"
            };
        }

        if (template.Blocks.Count == 0)
        {
            return new VoiceProcessResult
            {
                Matched = false,
                Error = "Template blocks are empty"
            };
        }

        var pauseProcessing = _pauseTextProcessor.Process(recognizedText);
        var textForMatching = pauseProcessing.ProcessedText;

        var tokens = _textNormalizer.Tokenize(textForMatching);

        if (tokens.Count == 0)
        {
            return new VoiceProcessResult
            {
                Matched = false,
                Error = "Recognized text is empty"
            };
        }

        var occurrences = BuildOccurrences(tokens, template);

        if (occurrences.Count == 0)
        {
            return new VoiceProcessResult
            {
                Matched = false,
                UnmatchedParts = [recognizedText],
                Error = "No keyword match found"
            };
        }

        var result = BuildResult(tokens, template, occurrences);

        if (result.Fields.Count == 0)
        {
            result.Matched = false;
            result.Error = "Keywords found, but no values extracted";
        }

        _autoTextGenerator.FillMissingAutoFields(result, template);

        return result;
    }

    private IReadOnlyList<KeywordOccurrence> BuildOccurrences(
        IReadOnlyList<TextToken> tokens,
        TemplateDto template)
    {
        var occurrences = new List<KeywordOccurrence>();
        TemplateBlockDto? currentBlock = null;

        for (var i = 0; i < tokens.Count; i++)
        {
            var blockMatch = _keywordMatcher.FindBestBlockMatch(tokens, template, i);

            if (blockMatch is not null)
            {
                occurrences.Add(blockMatch);
                currentBlock = FindBlock(template, blockMatch.BlockId);
                i += blockMatch.WordCount - 1;
                continue;
            }

            if (currentBlock is null)
                continue;

            var fieldMatch = _keywordMatcher.FindBestFieldMatch(tokens, currentBlock, i);

            if (fieldMatch is not null)
            {
                occurrences.Add(fieldMatch);
                i += fieldMatch.WordCount - 1;
            }
        }

        return RemoveOverlaps(occurrences);
    }

    private VoiceProcessResult BuildResult(
    IReadOnlyList<TextToken> tokens,
    TemplateDto template,
    IReadOnlyList<KeywordOccurrence> occurrences)
    {
        var result = new VoiceProcessResult
        {
            Matched = true
        };

        var matchedRanges = occurrences
            .Select(x => new MatchedTextRange
            {
                StartWordIndex = x.WordIndex,
                EndWordIndex = x.EndWordIndex
            })
            .ToList();

        for (var i = 0; i < occurrences.Count; i++)
        {
            var occurrence = occurrences[i];

            if (occurrence.Kind == KeywordMatchKind.Field)
            {
                var valueRange = AddFieldResult(
                    tokens,
                    template,
                    occurrences,
                    i,
                    occurrence,
                    result);

                if (valueRange is not null)
                    matchedRanges.Add(valueRange);

                continue;
            }

            if (occurrence.Kind == KeywordMatchKind.Block)
            {
                var valueRange = AddDefaultFieldResult(
                    tokens,
                    template,
                    occurrences,
                    i,
                    occurrence,
                    result);

                if (valueRange is not null)
                    matchedRanges.Add(valueRange);
            }
        }

        result.UnmatchedParts = BuildUnmatchedParts(tokens, matchedRanges);

        return result;
    }

    private MatchedTextRange? AddFieldResult(
    IReadOnlyList<TextToken> tokens,
    TemplateDto template,
    IReadOnlyList<KeywordOccurrence> occurrences,
    int occurrenceIndex,
    KeywordOccurrence occurrence,
    VoiceProcessResult result)
    {
        var block = FindBlock(template, occurrence.BlockId);

        if (block is null)
            return null;

        var field = block.Fields.FirstOrDefault(x => x.FieldName == occurrence.FieldName);

        if (field is null)
            return null;

        var valueStart = occurrence.EndWordIndex;
        var valueEnd = FindValueEnd(tokens.Count, occurrences, occurrenceIndex);
        var maxWords = field.Type == TemplateFieldType.Text ? MaxTextValueWords : MaxNumericValueWords;

        var rawValue = _valueExtractor.Extract(tokens, valueStart, valueEnd, maxWords);
        var normalizedValue = _valueNormalizer.Normalize(rawValue, field);

        if (string.IsNullOrWhiteSpace(normalizedValue.Value))
            return null;

        AddOrReplaceFieldResult(
            result,
            new MatchedFieldResult
            {
                BlockName = block.Name,
                FieldName = field.FieldName,
                Keyword = occurrence.Keyword,
                RecognizedKeyword = occurrence.RecognizedKeyword,
                RawValue = rawValue,
                Value = normalizedValue.Value,
                Confidence = Math.Round(occurrence.Confidence * normalizedValue.Confidence, 2),
                NormStatus = normalizedValue.NormStatus,
                NormMessage = normalizedValue.NormMessage
            });

        return new MatchedTextRange
        {
            StartWordIndex = valueStart,
            EndWordIndex = CalculateExtractedValueEnd(
                valueStart,
                valueEnd,
                rawValue)
        };
    }

    private MatchedTextRange? AddDefaultFieldResult(
    IReadOnlyList<TextToken> tokens,
    TemplateDto template,
    IReadOnlyList<KeywordOccurrence> occurrences,
    int occurrenceIndex,
    KeywordOccurrence occurrence,
    VoiceProcessResult result)
    {
        var block = FindBlock(template, occurrence.BlockId);

        if (block is null || string.IsNullOrWhiteSpace(block.DefaultFieldName))
            return null;

        var nextIndex = occurrenceIndex + 1;

        if (nextIndex < occurrences.Count &&
            occurrences[nextIndex].Kind == KeywordMatchKind.Field &&
            occurrences[nextIndex].BlockId == block.Id)
        {
            return null;
        }

        var field = block.Fields.FirstOrDefault(x => x.FieldName == block.DefaultFieldName);

        if (field is null)
            return null;

        var valueStart = occurrence.EndWordIndex;
        var valueEnd = FindValueEnd(tokens.Count, occurrences, occurrenceIndex);
        var maxWords = field.Type == TemplateFieldType.Text ? MaxTextValueWords : MaxNumericValueWords;

        var rawValue = _valueExtractor.Extract(tokens, valueStart, valueEnd, maxWords);
        var normalizedValue = _valueNormalizer.Normalize(rawValue, field);

        if (string.IsNullOrWhiteSpace(normalizedValue.Value))
            return null;

        AddOrReplaceFieldResult(
            result,
            new MatchedFieldResult
            {
                BlockName = block.Name,
                FieldName = field.FieldName,
                Keyword = occurrence.Keyword,
                RecognizedKeyword = occurrence.RecognizedKeyword,
                RawValue = rawValue,
                Value = normalizedValue.Value,
                Confidence = Math.Round(occurrence.Confidence * normalizedValue.Confidence, 2),
                NormStatus = normalizedValue.NormStatus,
                NormMessage = normalizedValue.NormMessage
            });

        return new MatchedTextRange
        {
            StartWordIndex = valueStart,
            EndWordIndex = CalculateExtractedValueEnd(
                valueStart,
                valueEnd,
                rawValue)
        };
    }

    private int CalculateExtractedValueEnd(
    int valueStart,
    int valueEnd,
    string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return valueStart;

        var valueTokens = _textNormalizer.Tokenize(rawValue);

        if (valueTokens.Count == 0)
            return valueStart;

        var calculatedEnd = valueStart + valueTokens.Count;

        return Math.Min(calculatedEnd, valueEnd);
    }

    private static int FindValueEnd(
        int tokensCount,
        IReadOnlyList<KeywordOccurrence> occurrences,
        int occurrenceIndex)
    {
        var nextIndex = occurrenceIndex + 1;

        if (nextIndex >= occurrences.Count)
            return tokensCount;

        return occurrences[nextIndex].WordIndex;
    }

    private static TemplateBlockDto? FindBlock(TemplateDto template, Guid blockId)
    {
        return template.Blocks.FirstOrDefault(x => x.Id == blockId);
    }

    private static IReadOnlyList<KeywordOccurrence> RemoveOverlaps(
        IReadOnlyList<KeywordOccurrence> occurrences)
    {
        var ordered = occurrences
            .OrderBy(x => x.WordIndex)
            .ThenByDescending(x => x.WordCount)
            .ThenByDescending(x => x.Confidence)
            .ToList();

        var result = new List<KeywordOccurrence>();
        var occupiedUntil = -1;

        foreach (var occurrence in ordered)
        {
            if (occurrence.WordIndex < occupiedUntil)
                continue;

            result.Add(occurrence);
            occupiedUntil = occurrence.EndWordIndex;
        }

        return result.OrderBy(x => x.WordIndex).ToList();
    }

    private static List<string> BuildUnmatchedParts(
    IReadOnlyList<TextToken> tokens,
    IReadOnlyList<MatchedTextRange> matchedRanges)
    {
        var unmatched = new List<string>();

        if (tokens.Count == 0)
            return unmatched;

        if (matchedRanges.Count == 0)
        {
            unmatched.Add(string.Join(' ', tokens.Select(x => x.Original)));
            return unmatched;
        }

        var mergedRanges = MergeRanges(matchedRanges);

        var currentIndex = 0;

        foreach (var range in mergedRanges)
        {
            if (currentIndex < range.StartWordIndex)
            {
                AddUnmatchedSegment(
                    tokens,
                    currentIndex,
                    range.StartWordIndex,
                    unmatched);
            }

            currentIndex = Math.Max(currentIndex, range.EndWordIndex);
        }

        if (currentIndex < tokens.Count)
        {
            AddUnmatchedSegment(
                tokens,
                currentIndex,
                tokens.Count,
                unmatched);
        }

        return unmatched
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<MatchedTextRange> MergeRanges(
    IReadOnlyList<MatchedTextRange> ranges)
    {
        var ordered = ranges
            .Where(x => x.StartWordIndex < x.EndWordIndex)
            .OrderBy(x => x.StartWordIndex)
            .ThenBy(x => x.EndWordIndex)
            .ToList();

        if (ordered.Count == 0)
            return [];

        var result = new List<MatchedTextRange>();

        var current = new MatchedTextRange
        {
            StartWordIndex = ordered[0].StartWordIndex,
            EndWordIndex = ordered[0].EndWordIndex
        };

        foreach (var range in ordered.Skip(1))
        {
            if (range.StartWordIndex <= current.EndWordIndex)
            {
                current.EndWordIndex = Math.Max(
                    current.EndWordIndex,
                    range.EndWordIndex);

                continue;
            }

            result.Add(current);

            current = new MatchedTextRange
            {
                StartWordIndex = range.StartWordIndex,
                EndWordIndex = range.EndWordIndex
            };
        }

        result.Add(current);

        return result;
    }

    private static void AddUnmatchedSegment(
    IReadOnlyList<TextToken> tokens,
    int start,
    int end,
    List<string> unmatched)
    {
        if (start >= end)
            return;

        var segment = string.Join(' ',
            tokens
                .Skip(start)
                .Take(end - start)
                .Select(x => x.Original));

        if (!string.IsNullOrWhiteSpace(segment))
            unmatched.Add(segment);
    }

    private static void AddOrReplaceFieldResult(
    VoiceProcessResult result,
    MatchedFieldResult field)
    {
        var existingIndex = result.Fields.FindIndex(x =>
            string.Equals(x.BlockName, field.BlockName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.FieldName, field.FieldName, StringComparison.OrdinalIgnoreCase));

        if (existingIndex >= 0)
        {
            result.Fields[existingIndex] = field;
            return;
        }

        result.Fields.Add(field);
    }

    private sealed class MatchedTextRange
    {
        public int StartWordIndex { get; set; }

        public int EndWordIndex { get; set; }
    }
}
