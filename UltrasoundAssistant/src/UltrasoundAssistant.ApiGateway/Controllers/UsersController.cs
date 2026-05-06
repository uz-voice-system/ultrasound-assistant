using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.ApiGateway.Services;
using UltrasoundAssistant.Contracts.Commands.Users;
using UltrasoundAssistant.Contracts.Reads.Users.Search;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[Route("api/users")]
public sealed class UsersController : GatewayControllerBase
{
    private readonly AggregationApiClient _aggregationClient;
    private readonly ProjectionApiClient _projectionClient;

    public UsersController(
        AggregationApiClient aggregationClient,
        ProjectionApiClient projectionClient)
    {
        _aggregationClient = aggregationClient;
        _projectionClient = projectionClient;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PostAsync("/api/users", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PutAsync("/api/users", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPatch("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateUserCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PatchAsync("/api/users/activate", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPatch("deactivate")]
    public async Task<IActionResult> Deactivate([FromBody] DeactivateUserCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PatchAsync("/api/users/deactivate", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _projectionClient.GetAsync($"/api/read/users/{id}" + Request.QueryString, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] UserSearchRequest filter, CancellationToken ct)
    {
        var result = await _projectionClient.PostAsync("/api/read/users/search", filter, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }
}
