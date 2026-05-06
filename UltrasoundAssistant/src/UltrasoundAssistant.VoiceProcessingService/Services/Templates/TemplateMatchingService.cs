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

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching;

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

        for (var i = 0; i < occurrences.Count; i++)
        {
            var occurrence = occurrences[i];

            if (occurrence.Kind == KeywordMatchKind.Field)
            {
                AddFieldResult(tokens, template, occurrences, i, occurrence, result);
                continue;
            }

            if (occurrence.Kind == KeywordMatchKind.Block)
            {
                AddDefaultFieldResult(tokens, template, occurrences, i, occurrence, result);
            }
        }

        result.UnmatchedParts = BuildUnmatchedParts(tokens, occurrences);

        return result;
    }

    private void AddFieldResult(
        IReadOnlyList<TextToken> tokens,
        TemplateDto template,
        IReadOnlyList<KeywordOccurrence> occurrences,
        int occurrenceIndex,
        KeywordOccurrence occurrence,
        VoiceProcessResult result)
    {
        var block = FindBlock(template, occurrence.BlockId);

        if (block is null)
            return;

        var field = block.Fields.FirstOrDefault(x => x.FieldName == occurrence.FieldName);

        if (field is null)
            return;

        var valueStart = occurrence.EndWordIndex;
        var valueEnd = FindValueEnd(tokens.Count, occurrences, occurrenceIndex);
        var maxWords = field.Type == TemplateFieldType.Text ? MaxTextValueWords : MaxNumericValueWords;

        var rawValue = _valueExtractor.Extract(tokens, valueStart, valueEnd, maxWords);
        var normalizedValue = _valueNormalizer.Normalize(rawValue, field);

        if (string.IsNullOrWhiteSpace(normalizedValue.Value))
            return;

        result.Fields.Add(new MatchedFieldResult
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
    }

    private void AddDefaultFieldResult(
        IReadOnlyList<TextToken> tokens,
        TemplateDto template,
        IReadOnlyList<KeywordOccurrence> occurrences,
        int occurrenceIndex,
        KeywordOccurrence occurrence,
        VoiceProcessResult result)
    {
        var block = FindBlock(template, occurrence.BlockId);

        if (block is null || string.IsNullOrWhiteSpace(block.DefaultFieldName))
            return;

        var nextIndex = occurrenceIndex + 1;

        if (nextIndex < occurrences.Count &&
            occurrences[nextIndex].Kind == KeywordMatchKind.Field &&
            occurrences[nextIndex].BlockId == block.Id)
        {
            return;
        }

        var field = block.Fields.FirstOrDefault(x => x.FieldName == block.DefaultFieldName);

        if (field is null)
            return;

        var valueStart = occurrence.EndWordIndex;
        var valueEnd = FindValueEnd(tokens.Count, occurrences, occurrenceIndex);
        var maxWords = field.Type == TemplateFieldType.Text ? MaxTextValueWords : MaxNumericValueWords;

        var rawValue = _valueExtractor.Extract(tokens, valueStart, valueEnd, maxWords);
        var normalizedValue = _valueNormalizer.Normalize(rawValue, field);

        if (string.IsNullOrWhiteSpace(normalizedValue.Value))
            return;

        result.Fields.Add(new MatchedFieldResult
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
        IReadOnlyList<KeywordOccurrence> occurrences)
    {
        var unmatched = new List<string>();

        if (occurrences.Count == 0)
        {
            unmatched.Add(string.Join(' ', tokens.Select(x => x.Original)));
            return unmatched;
        }

        if (occurrences[0].WordIndex > 0)
        {
            var prefix = string.Join(' ',
                tokens
                    .Take(occurrences[0].WordIndex)
                    .Select(x => x.Original));

            if (!string.IsNullOrWhiteSpace(prefix))
                unmatched.Add(prefix);
        }

        for (var i = 0; i < occurrences.Count; i++)
        {
            var start = occurrences[i].EndWordIndex;
            var end = i + 1 < occurrences.Count
                ? occurrences[i + 1].WordIndex
                : tokens.Count;

            if (start >= end)
                continue;

            var segment = string.Join(' ',
                tokens
                    .Skip(start)
                    .Take(end - start)
                    .Select(x => x.Original));

            if (!string.IsNullOrWhiteSpace(segment))
                unmatched.Add(segment);
        }

        return unmatched.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}
