using UltrasoundAssistant.Contracts.VoiceProcessing;
using UltrasoundAssistant.VoiceProcessingService.Domain;

namespace UltrasoundAssistant.VoiceProcessingService.Application.Validation;

public sealed class VoiceProcessRequestValidator
{
    public ValidationResult Validate(VoiceProcessRequest request)
    {
        if (request.TemplateId == Guid.Empty)
            return ValidationResult.Fail("TemplateId is required");

        if (string.IsNullOrWhiteSpace(request.AudioBase64))
            return ValidationResult.Fail("AudioBase64 is required");

        return ValidationResult.Success();
    }
}
