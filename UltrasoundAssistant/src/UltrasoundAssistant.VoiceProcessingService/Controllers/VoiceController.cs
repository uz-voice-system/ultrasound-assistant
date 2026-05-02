using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.VoiceProcessing;
using UltrasoundAssistant.VoiceProcessingService.Application.Abstractions;
using UltrasoundAssistant.VoiceProcessingService.Domain;

namespace UltrasoundAssistant.VoiceProcessingService.Controllers;

[ApiController]
[Route("api/voice")]
public sealed class VoiceController : ControllerBase
{
    private readonly IVoiceProcessingUseCase _voiceProcessingUseCase;

    public VoiceController(IVoiceProcessingUseCase voiceProcessingUseCase)
    {
        _voiceProcessingUseCase = voiceProcessingUseCase;
    }

    [HttpPost("process")]
    public async Task<ActionResult<VoiceProcessResult>> Process([FromBody] VoiceProcessRequest request, CancellationToken cancellationToken)
    {
        var result = await _voiceProcessingUseCase.ProcessAsync(request, cancellationToken);

        return result.StatusCode switch
        {
            VoiceProcessStatus.Ok => Ok(result.Value),
            VoiceProcessStatus.BadRequest => BadRequest(result.Value),
            VoiceProcessStatus.NotFound => NotFound(result.Value),
            _ => StatusCode(500, result.Value)
        };
    }
}
