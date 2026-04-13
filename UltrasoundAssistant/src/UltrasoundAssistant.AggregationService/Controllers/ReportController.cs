using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.AggregationService.Application.Handlers;
using UltrasoundAssistant.Contracts.Commands.Reports;

namespace UltrasoundAssistant.AggregationService.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportController : ControllerBase
{
    private readonly ReportCommandHandler _handler;

    public ReportController(ReportCommandHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateReportCommand command,
        CancellationToken ct)
    {
        var result = await _handler.CreateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("field")]
    public async Task<IActionResult> UpdateField(
        [FromBody] UpdateReportFieldCommand command,
        CancellationToken ct)
    {
        var result = await _handler.UpdateFieldAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("voice")]
    public async Task<IActionResult> ProcessVoice(
        [FromBody] ProcessVoiceDataCommand command,
        CancellationToken ct)
    {
        var result = await _handler.ProcessVoiceAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete(
        [FromBody] CompleteReportCommand command,
        CancellationToken ct)
    {
        var result = await _handler.CompleteAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteReportCommand command,
        CancellationToken ct)
    {
        var result = await _handler.DeleteAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}