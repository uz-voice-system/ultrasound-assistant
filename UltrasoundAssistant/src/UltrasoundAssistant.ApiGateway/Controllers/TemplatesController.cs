using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.ApiGateway.Services;
using UltrasoundAssistant.Contracts.Commands.Templates;
using UltrasoundAssistant.Contracts.Reads.Templates.Admin;
using UltrasoundAssistant.Contracts.Reads.Templates.Search;
using static System.Net.WebRequestMethods;

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

    [HttpPost("search")]
    public async Task<IActionResult> SearchForDoctor([FromBody] TemplateSearchRequest filter, CancellationToken ct)
    {
        var result = await _projectionClient.PostAsync("/api/read/templates/search", filter, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPost("search-admin")]
    public async Task<IActionResult> SearchForAdmin([FromBody] TemplateAdminSearchRequest filter, CancellationToken ct)
    {
        var result = await _projectionClient.PostAsync("/api/read/templates/search-admin", filter, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _projectionClient.GetAsync($"/api/read/templates/{id}" + Request.QueryString, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }
}
