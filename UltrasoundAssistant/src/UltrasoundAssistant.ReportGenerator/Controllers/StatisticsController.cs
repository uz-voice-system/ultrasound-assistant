using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Statistics;
using UltrasoundAssistant.ReportGenerator.Abstractions;

namespace UltrasoundAssistant.ReportGenerator.Controllers;

[ApiController]
[Route("api/generated-reports/statistics")]
public sealed class StatisticsController : ControllerBase
{
    private readonly IStatisticsDataClient _statisticsDataClient;
    private readonly IAdminStatisticsPdfGenerator _pdfGenerator;

    public StatisticsController(
        IStatisticsDataClient statisticsDataClient,
        IAdminStatisticsPdfGenerator pdfGenerator)
    {
        _statisticsDataClient = statisticsDataClient;
        _pdfGenerator = pdfGenerator;
    }

    [HttpPost("admin")]
    public async Task<ActionResult<AdminStatisticsDto>> GetAdminStatistics([FromBody] AdminStatisticsRequest request, CancellationToken cancellationToken)
    {
        var validationResult = ValidateRequest(request);

        if (validationResult is not null)
            return validationResult;

        var statistics = await _statisticsDataClient.GetAdminStatisticsAsync(
            request,
            cancellationToken);

        if (statistics is null)
            return NotFound(new { message = "Statistics not found" });

        return Ok(statistics);
    }

    [HttpPost("admin/pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GenerateAdminStatisticsPdf([FromBody] AdminStatisticsRequest request, CancellationToken cancellationToken)
    {
        var validationResult = ValidateRequest(request);

        if (validationResult is not null)
            return validationResult;

        var statistics = await _statisticsDataClient.GetAdminStatisticsAsync(
            request,
            cancellationToken);

        if (statistics is null)
            return NotFound(new { message = "Statistics not found" });

        var pdf = _pdfGenerator.Generate(statistics);

        var fileName =
            $"admin-statistics-{request.DateFromUtc:yyyyMMdd}-{request.DateToUtc:yyyyMMdd}.pdf";

        return File(pdf, "application/pdf", fileName);
    }

    private BadRequestObjectResult? ValidateRequest(AdminStatisticsRequest request)
    {
        if (request.DateFromUtc == default)
            return BadRequest(new { message = "DateFromUtc is required" });

        if (request.DateToUtc == default)
            return BadRequest(new { message = "DateToUtc is required" });

        if (request.DateFromUtc > request.DateToUtc)
            return BadRequest(new { message = "DateFromUtc cannot be greater than DateToUtc" });

        return null;
    }
}