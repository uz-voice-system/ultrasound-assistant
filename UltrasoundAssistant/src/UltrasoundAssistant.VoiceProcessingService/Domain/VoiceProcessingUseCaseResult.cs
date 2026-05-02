using UltrasoundAssistant.Contracts.VoiceProcessing;

namespace UltrasoundAssistant.VoiceProcessingService.Domain;

public sealed class VoiceProcessingUseCaseResult
{
    public VoiceProcessStatus StatusCode { get; init; }
    public VoiceProcessResult Value { get; init; } = new();
}

public enum VoiceProcessStatus
{
    Ok,
    BadRequest,
    NotFound,
    Error
}
