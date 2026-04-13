using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.AggregationService.Application.Handlers;
using UltrasoundAssistant.Contracts.Commands.Templates;

namespace UltrasoundAssistant.AggregationService.Controllers;

[ApiController]
[Route("api/templates")]
public sealed class TemplateController : ControllerBase
{
    private readonly TemplateCommandHandler _handler;

    public TemplateController(TemplateCommandHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTemplateCommand command,
        CancellationToken ct)
    {
        var result = await _handler.CreateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] UpdateTemplateCommand command,
        CancellationToken ct)
    {
        var result = await _handler.UpdateAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteTemplateCommand command,
        CancellationToken ct)
    {
        var result = await _handler.DeleteAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}