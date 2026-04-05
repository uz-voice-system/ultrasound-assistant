using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Commands.Patients;
using UltrasoundAssistant.Contracts.Events.CommandEvent;
using UltrasoundAssistant.Contracts.Patients;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[ApiController]
[Route("api/patients")]
public sealed class PatientsGatewayController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-Command-Id")] Guid? commandId,
        [FromBody] CreatePatientRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Body is required" });
        }

        var cmdId = commandId ?? Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var command = new CreatePatientCommand
        {
            Id = patientId,
            FullName = request.FullName,
            BirthDate = request.BirthDate,
            Gender = request.Gender
        };

        var client = httpClientFactory.CreateClient("Aggregation");
        using var message = new HttpRequestMessage(HttpMethod.Post, "commands/patients/create")
        {
            Content = JsonContent.Create(command)
        };
        message.Headers.Add("X-Command-Id", cmdId.ToString());
        var response = await client.SendAsync(message, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromHeader(Name = "X-Command-Id")] Guid? commandId,
        [FromBody] UpdatePatientRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Body is required" });
        }

        var command = new UpdatePatientCommand
        {
            CommandId = commandId ?? Guid.NewGuid(),
            PatientId = id,
            ExpectedVersion = request.ExpectedVersion,
            FullName = request.FullName,
            BirthDate = request.BirthDate,
            Gender = request.Gender
        };

        var client = httpClientFactory.CreateClient("Aggregation");
        var response = await client.PostAsJsonAsync("commands/patients/update", command, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        [FromBody] DeactivatePatientCommand? command,
        CancellationToken cancellationToken)
    {
        if (command is null)
        {
            return BadRequest(new { message = "Body is required" });
        }

        command.PatientId = id;
        if (command.CommandId == Guid.Empty)
        {
            command.CommandId = Guid.NewGuid();
        }

        var client = httpClientFactory.CreateClient("Aggregation");
        var response = await client.PostAsJsonAsync("commands/patients/deactivate", command, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Projection");
        var response = await client.GetAsync($"api/read/patients/{id}", cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Projection");
        var response = await client.GetAsync("api/read/patients", cancellationToken);
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
