using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.AggregationService.Application.Handlers;
using UltrasoundAssistant.Contracts.Commands.Schedules;

namespace UltrasoundAssistant.AggregationService.Controllers;

[ApiController]
[Route("api/schedules")]
public sealed class UserScheduleController : ControllerBase
{
    private readonly UserScheduleCommandHandler _handler;

    public UserScheduleController(UserScheduleCommandHandler handler)
    {
        _handler = handler;
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserScheduleCommand command, CancellationToken ct)
    {
        var result = await _handler.UpdateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}
