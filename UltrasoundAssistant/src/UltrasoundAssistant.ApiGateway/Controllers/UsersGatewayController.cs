using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Auth;
using UltrasoundAssistant.Contracts.Commands.Users;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersGatewayController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-Command-Id")] Guid? commandId,
        [FromBody] CreateUserRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Body is required" });
        }

        var command = new CreateUserCommand
        {
            CommandId = commandId ?? Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Login = request.Login,
            Password = request.Password,
            Role = request.Role
        };

        var client = httpClientFactory.CreateClient("Aggregation");
        var response = await client.PostAsJsonAsync("commands/users/create", command, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromHeader(Name = "X-Command-Id")] Guid? commandId,
        [FromBody] UpdateUserRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Body is required" });
        }

        var command = new UpdateUserCommand
        {
            CommandId = commandId ?? Guid.NewGuid(),
            UserId = id,
            ExpectedVersion = request.ExpectedVersion,
            Login = request.Login,
            Password = request.Password,
            Role = request.Role
        };

        var client = httpClientFactory.CreateClient("Aggregation");
        var response = await client.PostAsJsonAsync("commands/users/update", command, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        [FromBody] DeactivateUserRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Body is required" });
        }

        var command = new DeactivateUserCommand
        {
            CommandId = request.CommandId == Guid.Empty ? Guid.NewGuid() : request.CommandId,
            UserId = id,
            ExpectedVersion = request.ExpectedVersion
        };

        var client = httpClientFactory.CreateClient("Aggregation");
        var response = await client.PostAsJsonAsync("commands/users/deactivate", command, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Projection");
        var response = await client.GetAsync($"api/read/users/{id}", cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("Projection");
        var url = includeInactive ? "api/read/users?includeInactive=true" : "api/read/users";
        var response = await client.GetAsync(url, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    private static async Task<IActionResult> ForwardResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return new ContentResult
        {
            StatusCode = (int)response.StatusCode,
            Content = body,
            ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/json"
        };
    }
}
