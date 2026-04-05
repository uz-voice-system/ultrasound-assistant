using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Commands.Reports;
using UltrasoundAssistant.Contracts.Events.CommandEvent;
using UltrasoundAssistant.Contracts.Reports;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsGatewayController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-Command-Id")] Guid? commandId,
        [FromBody] CreateReportRequest request,
        CancellationToken cancellationToken)
    {
        var cmdId = commandId ?? Guid.NewGuid();
        var reportId = request.ReportId ?? Guid.NewGuid();
        var command = new CreateReportCommand
        {
            CommandId = cmdId,
            ReportId = reportId,
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            TemplateId = request.TemplateId
        };

        var client = httpClientFactory.CreateClient("Aggregation");
        var response = await client.PostAsJsonAsync("commands/reports/create", command, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpPatch("{id:guid}/field")]
    public async Task<IActionResult> UpdateField(
        Guid id,
        [FromHeader(Name = "X-Command-Id")] Guid? commandId,
        [FromBody] UpdateReportFieldRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateReportFieldCommand
        {
            CommandId = commandId ?? Guid.NewGuid(),
            ReportId = id,
            ExpectedVersion = request.ExpectedVersion,
            FieldName = request.FieldName,
            Value = request.Value,
            Confidence = request.Confidence
        };

        var client = httpClientFactory.CreateClient("Aggregation");
        var response = await client.PostAsJsonAsync("commands/reports/update-field", command, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteReportCommand command, CancellationToken cancellationToken)
    {
        command.ReportId = id;
        var client = httpClientFactory.CreateClient("Aggregation");
        var response = await client.PostAsJsonAsync("commands/reports/complete", command, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteReportRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Body is required" });
        }

        var command = new DeleteReportCommand
        {
            CommandId = request.CommandId == Guid.Empty ? Guid.NewGuid() : request.CommandId,
            ReportId = id,
            ExpectedVersion = request.ExpectedVersion
        };

        var client = httpClientFactory.CreateClient("Aggregation");
        var response = await client.PostAsJsonAsync("commands/reports/delete", command, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpPost("{id:guid}/process-voice")]
    public async Task<IActionResult> ProcessVoice(
        Guid id,
        [FromHeader(Name = "X-Command-Id")] Guid commandId,
        [FromHeader(Name = "X-Expected-Version")] int expectedVersion,
        [FromBody] ProcessVoiceDataCommand command,
        CancellationToken cancellationToken)
    {
        command.ReportId = id;
        var client = httpClientFactory.CreateClient("Aggregation");
        using var message = new HttpRequestMessage(HttpMethod.Post, "commands/reports/process-voice")
        {
            Content = JsonContent.Create(command)
        };
        message.Headers.Add("X-Command-Id", commandId.ToString());
        message.Headers.Add("X-Expected-Version", expectedVersion.ToString());
        var response = await client.SendAsync(message, cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Projection");
        var response = await client.GetAsync($"api/read/reports/{id}", cancellationToken);
        return await ForwardResponseAsync(response, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Projection");
        var response = await client.GetAsync("api/read/reports", cancellationToken);
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
