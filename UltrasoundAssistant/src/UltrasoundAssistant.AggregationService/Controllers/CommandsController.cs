using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.AggregationService.Services;
using UltrasoundAssistant.Contracts.Commands.Patients;
using UltrasoundAssistant.Contracts.Commands.Reports;
using UltrasoundAssistant.Contracts.Commands.Templates;
using UltrasoundAssistant.Contracts.Commands.Users;
using UltrasoundAssistant.Contracts.Events.CommandEvent;

namespace UltrasoundAssistant.AggregationService.Controllers;

[ApiController]
[Route("commands")]
public sealed class CommandsController(CommandService commandService) : ControllerBase
{
    [HttpPost("patients/create")]
    public async Task<IActionResult> CreatePatient(
        [FromBody] CreatePatientCommand command,
        [FromHeader(Name = "X-Command-Id")] Guid commandId,
        CancellationToken cancellationToken)
    {
        var result = await commandService.CreatePatientAsync(command, commandId, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("patients/update")]
    public async Task<IActionResult> UpdatePatient([FromBody] UpdatePatientCommand command, CancellationToken cancellationToken)
    {
        var result = await commandService.UpdatePatientAsync(command, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("patients/deactivate")]
    public async Task<IActionResult> DeactivatePatient([FromBody] DeactivatePatientCommand command, CancellationToken cancellationToken)
    {
        var result = await commandService.DeactivatePatientAsync(command, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("users/create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await commandService.CreateUserAsync(command, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("users/update")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await commandService.UpdateUserAsync(command, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("users/deactivate")]
    public async Task<IActionResult> DeactivateUser([FromBody] DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await commandService.DeactivateUserAsync(command, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("templates/create")]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateCommand command, CancellationToken cancellationToken)
    {
        var result = await commandService.CreateTemplateAsync(command, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("templates/update")]
    public async Task<IActionResult> UpdateTemplate([FromBody] UpdateTemplateCommand command, CancellationToken cancellationToken)
    {
        var result = await commandService.UpdateTemplateAsync(command, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("templates/delete")]
    public async Task<IActionResult> DeleteTemplate([FromBody] DeleteTemplateCommand command, CancellationToken cancellationToken)
    {
        var result = await commandService.DeleteTemplateAsync(command, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("reports/create")]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportCommand command, CancellationToken cancellationToken)
    {
        var result = await commandService.CreateReportAsync(command, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("reports/update-field")]
    public async Task<IActionResult> UpdateReportField([FromBody] UpdateReportFieldCommand command, CancellationToken cancellationToken)
    {
        var result = await commandService.UpdateReportFieldAsync(command, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("reports/delete")]
    public async Task<IActionResult> DeleteReport([FromBody] DeleteReportCommand command, CancellationToken cancellationToken)
    {
        var result = await commandService.DeleteReportAsync(command, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("reports/process-voice")]
    public async Task<IActionResult> ProcessVoice(
        [FromBody] ProcessVoiceDataCommand command,
        [FromHeader(Name = "X-Command-Id")] Guid commandId,
        [FromHeader(Name = "X-Expected-Version")] int expectedVersion,
        CancellationToken cancellationToken)
    {
        var result = await commandService.ProcessVoiceAsync(command, commandId, expectedVersion, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("reports/complete")]
    public async Task<IActionResult> CompleteReport([FromBody] CompleteReportCommand command, CancellationToken cancellationToken)
    {
        var result = await commandService.CompleteReportAsync(command, cancellationToken);
        return StatusCode(result.StatusCode, new { message = result.Message });
    }
}
