using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.AggregationService.Application.Handlers;
using UltrasoundAssistant.Contracts.Commands.Users;

namespace UltrasoundAssistant.AggregationService.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UserController : ControllerBase
{
    private readonly UserCommandHandler _handler;

    public UserController(UserCommandHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var result = await _handler.CreateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserCommand command, CancellationToken ct)
    {
        var result = await _handler.UpdateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateUserCommand command, CancellationToken ct)
    {
        var result = await _handler.ActivateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("deactivate")]
    public async Task<IActionResult> Deactivate([FromBody] DeactivateUserCommand command, CancellationToken ct)
    {
        var result = await _handler.DeactivateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}
