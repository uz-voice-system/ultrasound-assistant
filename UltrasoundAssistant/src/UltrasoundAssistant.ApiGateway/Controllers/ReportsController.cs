using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.ApiGateway.Services;
using UltrasoundAssistant.Contracts.Commands.Reports;
using UltrasoundAssistant.Contracts.Reads.Reports.Search;
using static System.Net.WebRequestMethods;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[Route("api/reports")]
public sealed class ReportsController : GatewayControllerBase
{
    private readonly AggregationApiClient _aggregationClient;
    private readonly ProjectionApiClient _projectionClient;

    public ReportsController(
        AggregationApiClient aggregationClient,
        ProjectionApiClient projectionClient)
    {
        _aggregationClient = aggregationClient;
        _projectionClient = projectionClient;
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
}
