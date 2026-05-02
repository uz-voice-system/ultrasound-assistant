using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.ApiGateway.Services;
using UltrasoundAssistant.Contracts.VoiceProcessing;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[Route("api/voice")]
public sealed class VoiceController : GatewayControllerBase
{
    private readonly VoiceProcessingApiClient _voiceClient;

    public VoiceController(VoiceProcessingApiClient voiceClient)
    {
        _voiceClient = voiceClient;
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] VoiceProcessRequest request, CancellationToken ct)
    {
        var result = await _voiceClient.PostAsync("/api/voice/process", request, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }
}