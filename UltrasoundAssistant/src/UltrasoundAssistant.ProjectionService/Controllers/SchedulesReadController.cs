using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Reads.Schedules.Details;
using UltrasoundAssistant.Contracts.Reads.Schedules.Search;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/schedules")]
public sealed class SchedulesReadController : ControllerBase
{
    private readonly IUserScheduleReadService _scheduleReadService;

    public SchedulesReadController(IUserScheduleReadService scheduleReadService)
    {
        _scheduleReadService = scheduleReadService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserScheduleDto>> GetById(
        Guid id,
        [FromQuery] bool includeDeleted,
        CancellationToken cancellationToken)
    {
        var schedule = await _scheduleReadService.GetByIdAsync(
            id,
            includeDeleted,
            cancellationToken);

        if (schedule is null)
            return NotFound();

        return Ok(schedule);
    }

    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<UserScheduleDto>>> GetByUserId(
        Guid userId,
        [FromQuery] bool includeDeleted,
        CancellationToken cancellationToken)
    {
        var schedules = await _scheduleReadService.GetByUserIdAsync(
            userId,
            includeDeleted,
            cancellationToken);

        return Ok(schedules);
    }

    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<UserScheduleSummaryDto>>> Search(
        [FromBody] UserScheduleSearchRequest filter,
        CancellationToken cancellationToken)
    {
        var schedules = await _scheduleReadService.SearchAsync(filter, cancellationToken);

        return Ok(schedules);
    }
}
