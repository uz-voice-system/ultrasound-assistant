namespace UltrasoundAssistant.Contracts.VoiceProcessing;

public sealed class VoiceProcessResult
{
    public bool Matched { get; set; }

    public List<MatchedFieldResult> Fields { get; set; } = [];

    public List<string> UnmatchedParts { get; set; } = [];

    public string? Error { get; set; }
}
