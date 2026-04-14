using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.ApiGateway.Services;
using UltrasoundAssistant.Contracts.Commands.Reports;

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

    [HttpPost("field")]
    public async Task<IActionResult> UpdateField([FromBody] UpdateReportFieldCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PostAsync("/api/reports/field", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPost("voice")]
    public async Task<IActionResult> ProcessVoice([FromBody] ProcessVoiceDataCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PostAsync("/api/reports/voice", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteReportCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PostAsync("/api/reports/complete", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeleteReportCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.DeleteAsync("/api/reports", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _projectionClient.GetAsync("/api/read/reports", ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _projectionClient.GetAsync($"/api/read/reports/{id}", ct);
        return ProxyJson(result.StatusCode, result.Content);
    }
}