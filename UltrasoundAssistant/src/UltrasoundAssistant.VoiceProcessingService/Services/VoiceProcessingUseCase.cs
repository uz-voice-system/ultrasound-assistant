using UltrasoundAssistant.Contracts.VoiceProcessing;
using UltrasoundAssistant.VoiceProcessingService.Application.Abstractions;
using UltrasoundAssistant.VoiceProcessingService.Application.Validation;
using UltrasoundAssistant.VoiceProcessingService.Domain;

namespace UltrasoundAssistant.VoiceProcessingService.Services;

public sealed class VoiceProcessingUseCase : IVoiceProcessingUseCase
{
    private readonly IWhisperTranscriptionService _transcriptionService;
    private readonly ITemplateLookupService _templateLookupService;
    private readonly ITemplateMatchingService _matchingService;
    private readonly VoiceProcessRequestValidator _validator;

    public VoiceProcessingUseCase(
        IWhisperTranscriptionService transcriptionService,
        ITemplateLookupService templateLookupService,
        ITemplateMatchingService matchingService,
        VoiceProcessRequestValidator validator)
    {
        _transcriptionService = transcriptionService;
        _templateLookupService = templateLookupService;
        _matchingService = matchingService;
        _validator = validator;
    }

    public async Task<VoiceProcessingUseCaseResult> ProcessAsync(VoiceProcessRequest request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);
        if (!validation.IsValid) return BadRequest(validation.Error);

        var template = await _templateLookupService.GetTemplateAsync(request.TemplateId, cancellationToken);

        if (template is null) return NotFound("Template not found");

        byte[] audioBytes;

        try
        {
            audioBytes = Convert.FromBase64String(request.AudioBase64);
        }
        catch
        {
            return BadRequest("AudioBase64 is invalid");
        }

        if (!AudioValidation.LooksLikeWav(audioBytes))
            return BadRequest("Only WAV audio is supported. Expected PCM mono 16kHz WAV.");

        var recognizedText = await _transcriptionService.TranscribeAsync(
            audioBytes,
            request.Language,
            cancellationToken);

        var result = _matchingService.Match(recognizedText, template);

        return new VoiceProcessingUseCaseResult
        {
            StatusCode = VoiceProcessStatus.Ok,
            Value = result
        };
    }

    private static VoiceProcessingUseCaseResult BadRequest(string error)
    {
        return new VoiceProcessingUseCaseResult
        {
            StatusCode = VoiceProcessStatus.BadRequest,
            Value = new VoiceProcessResult
            {
                Matched = false,
                Error = error
            }
        };
    }

    private static VoiceProcessingUseCaseResult NotFound(string error)
    {
        return new VoiceProcessingUseCaseResult
        {
            StatusCode = VoiceProcessStatus.NotFound,
            Value = new VoiceProcessResult
            {
                Matched = false,
                Error = error
            }
        };
    }
}
