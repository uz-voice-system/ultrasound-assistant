using Microsoft.AspNetCore.Mvc;
using System.Text;
using UltrasoundAssistant.ApiGateway.Contracts;
using UltrasoundAssistant.ApiGateway.Services;
using UltrasoundAssistant.Contracts.Commands.Reports;
using UltrasoundAssistant.Contracts.Reads.Reports.Search;
using UltrasoundAssistant.Contracts.Statistics;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[Route("api/reports")]
public sealed class ReportsController : GatewayControllerBase
{
    private readonly AggregationApiClient _aggregationClient;
    private readonly ProjectionApiClient _projectionClient;
    private readonly ReportGeneratorClient _reportGeneratorClient;

    public ReportsController(
        AggregationApiClient aggregationClient,
        ProjectionApiClient projectionClient,
        ReportGeneratorClient reportGeneratorClient)
    {
        _aggregationClient = aggregationClient;
        _projectionClient = projectionClient;
        _reportGeneratorClient = reportGeneratorClient;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReportCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PostAsync("/api/reports", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateReportCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PutAsync("/api/reports", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeleteReportCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.DeleteAsync("/api/reports", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _projectionClient.GetAsync($"/api/read/reports/{id}" + Request.QueryString, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet("by-appointment/{appointmentId:guid}")]
    public async Task<IActionResult> GetByAppointmentId(Guid appointmentId, CancellationToken ct)
    {
        var result = await _projectionClient.GetAsync($"/api/read/reports/by-appointment/{appointmentId}", ct);

        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] ReportSearchRequest filter, CancellationToken ct)
    {
        var result = await _projectionClient.PostAsync("/api/read/reports/search", filter, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet("{id:guid}/pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GeneratePdf(Guid id, CancellationToken ct)
    {
        var result = await _reportGeneratorClient.GetReportPdfAsync(id, ct);

        if (result.StatusCode is >= 200 and < 300)
        {
            return File(
                result.Content,
                result.ContentType,
                result.FileName);
        }

        var errorContent = System.Text.Encoding.UTF8.GetString(result.Content);

        return ProxyJson(result.StatusCode, errorContent);
    }

    [HttpPut("{id:guid}/image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(Guid id, [FromForm] UploadReportImageForm form, CancellationToken ct)
    {
        if (form.File is null)
            return BadRequest(new { message = "Image file is required" });

        if (form.File.Length == 0)
            return BadRequest(new { message = "Image file is empty" });

        await using var stream = form.File.OpenReadStream();

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, ct);

        var command = new UploadReportImageCommand
        {
            ReportId = id,
            FileName = form.File.FileName,
            ContentType = form.File.ContentType,
            ImageBase64 = Convert.ToBase64String(memoryStream.ToArray()),
            ExpectedVersion = form.ExpectedVersion
        };

        var result = await _aggregationClient.PutAsync("/api/reports/image", command, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpDelete("{id:guid}/image")]
    public async Task<IActionResult> DeleteImage(Guid id, [FromBody] DeleteReportImageCommand command, CancellationToken ct)
    {
        command.ReportId = id;

        var result = await _aggregationClient.DeleteAsync("/api/reports/image", command, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPost("statistics/admin")]
    public async Task<IActionResult> GetAdminStatistics([FromBody] AdminStatisticsRequest request, CancellationToken ct)
    {
        var result = await _reportGeneratorClient.GetAdminStatisticsAsync(request, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPost("statistics/admin/pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetAdminStatisticsPdf([FromBody] AdminStatisticsRequest request, CancellationToken ct)
    {
        var result = await _reportGeneratorClient.GetAdminStatisticsPdfAsync(request, ct);

        if (result.StatusCode is >= 200 and < 300)
        {
            return File(
                result.Content,
                result.ContentType,
                result.FileName);
        }

        var errorContent = Encoding.UTF8.GetString(result.Content);

        return ProxyJson(result.StatusCode, errorContent);
    }
}
