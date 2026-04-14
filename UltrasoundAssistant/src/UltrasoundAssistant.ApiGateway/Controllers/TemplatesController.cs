using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.ApiGateway.Services;
using UltrasoundAssistant.Contracts.Commands.Templates;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[Route("api/templates")]
public sealed class TemplatesController : GatewayControllerBase
{
    private readonly AggregationApiClient _aggregationClient;
    private readonly ProjectionApiClient _projectionClient;

    public TemplatesController(
        AggregationApiClient aggregationClient,
        ProjectionApiClient projectionClient)
    {
        _aggregationClient = aggregationClient;
        _projectionClient = projectionClient;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTemplateCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PostAsync("/api/templates", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateTemplateCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PutAsync("/api/templates", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeleteTemplateCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.DeleteAsync("/api/templates", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _projectionClient.GetAsync("/api/read/templates", ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _projectionClient.GetAsync($"/api/read/templates/{id}", ct);
        return ProxyJson(result.StatusCode, result.Content);
    }
}