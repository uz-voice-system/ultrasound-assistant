namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class VoiceCommandMatcher
{
    public VoiceMatchResult Match(
        string recognizedText,
        IReadOnlyDictionary<string, string> templateKeywords)
    {
        if (string.IsNullOrWhiteSpace(recognizedText))
            return VoiceMatchResult.Fail("Recognized text is empty");

        if (templateKeywords is null || templateKeywords.Count == 0)
            return VoiceMatchResult.Fail("Template keywords are missing");

        var text = recognizedText.Trim();

        foreach (var pair in templateKeywords.OrderByDescending(x => x.Key.Length))
        {
            var phrase = pair.Key.Trim();
            if (string.IsNullOrWhiteSpace(phrase))
                continue;

            var idx = text.IndexOf(phrase, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                continue;

            var valueStart = idx + phrase.Length;
            var value = text[valueStart..].Trim().Trim(':', '-', ' ');

            if (string.IsNullOrWhiteSpace(value))
                continue;

            return VoiceMatchResult.Success(pair.Value, value);
        }

        var delimiter = text.IndexOf(':');
        if (delimiter > 0 && delimiter < text.Length - 1)
        {
            var spokenPhrase = text[..delimiter].Trim();

            if (templateKeywords.TryGetValue(spokenPhrase, out var targetField))
            {
                var value = text[(delimiter + 1)..].Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    return VoiceMatchResult.Success(targetField, value);
            }
        }

        return VoiceMatchResult.Fail("Voice command does not match template keywords");
    }
}

public sealed class VoiceMatchResult
{
    public bool IsSuccess { get; }
    public string FieldName { get; }
    public string Value { get; }
    public string Error { get; }

    private VoiceMatchResult(bool isSuccess, string fieldName, string value, string error)
    {
        IsSuccess = isSuccess;
        FieldName = fieldName;
        Value = value;
        Error = error;
    }

    public static VoiceMatchResult Success(string fieldName, string value)
        => new(true, fieldName, value, string.Empty);

    public static VoiceMatchResult Fail(string error)
        => new(false, string.Empty, string.Empty, error);
}