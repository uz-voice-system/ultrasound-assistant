using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.AggregationService.Application.Handlers;
using UltrasoundAssistant.Contracts.Commands.Appointments;

namespace UltrasoundAssistant.AggregationService.Controllers;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentController : ControllerBase
{
    private readonly AppointmentCommandHandler _handler;

    public AppointmentController(AppointmentCommandHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentCommand command, CancellationToken ct)
    {
        var result = await _handler.CreateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateAppointmentCommand command, CancellationToken ct)
    {
        var result = await _handler.UpdateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeleteAppointmentCommand command, CancellationToken ct)
    {
        var result = await _handler.DeleteAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}
