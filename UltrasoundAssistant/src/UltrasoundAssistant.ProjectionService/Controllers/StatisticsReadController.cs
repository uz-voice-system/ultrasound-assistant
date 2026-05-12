using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Statistics;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/statistics")]
public sealed class StatisticsReadController : ControllerBase
{
    private readonly IAdminStatisticsReadService _statisticsReadService;

    public StatisticsReadController(IAdminStatisticsReadService statisticsReadService)
    {
        _statisticsReadService = statisticsReadService;
    }

    [HttpPost("admin")]
    public async Task<ActionResult<AdminStatisticsDto>> GetAdminStatistics(
        [FromBody] AdminStatisticsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var statistics = await _statisticsReadService.GetAsync(request, cancellationToken);
            return Ok(statistics);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
