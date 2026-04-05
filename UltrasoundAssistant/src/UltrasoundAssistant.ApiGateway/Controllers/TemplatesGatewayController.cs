using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Commands.Templates;
using UltrasoundAssistant.Contracts.Templates;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[ApiController]
[Route("api/templates")]
public sealed class TemplatesGatewayController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTemplateCommand command, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Aggregation");
        var response = await client.PostAsJsonAsync("commands/templates/create", command, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateCommand command, CancellationToken cancellationToken)
    {
        command.TemplateId = id;
        var client = httpClientFactory.CreateClient("Aggregation");
        var response = await client.PostAsJsonAsync("commands/templates/update", command, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteTemplateRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Body is required" });
        }

        var command = new DeleteTemplateCommand
        {
            CommandId = request.CommandId == Guid.Empty ? Guid.NewGuid() : request.CommandId,
            TemplateId = id,
            ExpectedVersion = request.ExpectedVersion
        };

        var client = httpClientFactory.CreateClient("Aggregation");
        var response = await client.PostAsJsonAsync("commands/templates/delete", command, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Projection");
        var response = await client.GetAsync($"api/read/templates/{id}", cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Projection");
        var response = await client.GetAsync("api/read/templates", cancellationToken);
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
