using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.ApiGateway.Services;
using UltrasoundAssistant.Contracts.Commands.Schedules;
using UltrasoundAssistant.Contracts.Reads.Schedules.Search;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[Route("api/schedules")]
public sealed class SchedulesController : GatewayControllerBase
{
    private readonly AggregationApiClient _aggregationClient;
    private readonly ProjectionApiClient _projectionClient;

    public SchedulesController(
        AggregationApiClient aggregationClient,
        ProjectionApiClient projectionClient)
    {
        _aggregationClient = aggregationClient;
        _projectionClient = projectionClient;
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserScheduleCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PutAsync("/api/schedules", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _projectionClient.GetAsync($"/api/read/schedules/{id}" + Request.QueryString, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet("by-user/{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken ct)
    {
        var result = await _projectionClient.GetAsync($"/api/read/schedules/by-user/{userId}" + Request.QueryString, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] UserScheduleSearchRequest filter, CancellationToken ct)
    {
        var result = await _projectionClient.PostAsync("/api/read/schedules/search", filter, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }
}
