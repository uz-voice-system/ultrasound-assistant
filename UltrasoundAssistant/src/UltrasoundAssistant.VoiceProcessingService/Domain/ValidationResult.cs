namespace UltrasoundAssistant.VoiceProcessingService.Domain;

public sealed class ValidationResult
{
    public bool IsValid { get; init; }
    public string Error { get; init; } = string.Empty;

    public static ValidationResult Success()
    {
        return new ValidationResult { IsValid = true };
    }

    public static ValidationResult Fail(string error)
    {
        return new ValidationResult
        {
            IsValid = false,
            Error = error
        };
    }
}
