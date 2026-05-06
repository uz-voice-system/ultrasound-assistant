using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Reads.Reports.Details;
using UltrasoundAssistant.Contracts.Reads.Reports.Search;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/reports")]
public sealed class ReportsReadController : ControllerBase
{
    private readonly IReportReadService _reportReadService;

    public ReportsReadController(IReportReadService reportReadService)
    {
        _reportReadService = reportReadService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReportDto>> GetById(
        Guid id,
        [FromQuery] bool includeDeleted,
        CancellationToken cancellationToken)
    {
        var report = await _reportReadService.GetByIdAsync(
            id,
            includeDeleted,
            cancellationToken);

        if (report is null)
            return NotFound();

        return Ok(report);
    }

    [HttpGet("by-appointment/{appointmentId:guid}")]
    public async Task<ActionResult<ReportDto>> GetByAppointmentId(
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var report = await _reportReadService.GetByAppointmentIdAsync(
            appointmentId,
            cancellationToken);

        if (report is null)
            return NotFound();

        return Ok(report);
    }

    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<ReportSummaryDto>>> Search(
        [FromBody] ReportSearchRequest filter,
        CancellationToken cancellationToken)
    {
        var reports = await _reportReadService.SearchAsync(
            filter,
            cancellationToken);

        return Ok(reports);
    }
}
