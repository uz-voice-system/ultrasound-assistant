using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.AggregationService.Application.Handlers;
using UltrasoundAssistant.Contracts.Commands.Patients;

namespace UltrasoundAssistant.AggregationService.Controllers;

[ApiController]
[Route("api/patients")]
public sealed class PatientController : ControllerBase
{
    private readonly PatientCommandHandler _handler;

    public PatientController(PatientCommandHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePatientCommand command,
        CancellationToken ct)
    {
        var result = await _handler.CreateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] UpdatePatientCommand command,
        CancellationToken ct)
    {
        var result = await _handler.UpdateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete]
    public async Task<IActionResult> Deactivate(
        [FromBody] DeactivatePatientCommand command,
        CancellationToken ct)
    {
        var result = await _handler.DeactivateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}