using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.ApiGateway.Services;
using UltrasoundAssistant.Contracts.Commands.Appointments;
using UltrasoundAssistant.Contracts.Reads.Appointments.Search;
using static System.Net.WebRequestMethods;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[Route("api/appointments")]
public sealed class AppointmentsController : GatewayControllerBase
{
    private readonly AggregationApiClient _aggregationClient;
    private readonly ProjectionApiClient _projectionClient;

    public AppointmentsController(
        AggregationApiClient aggregationClient,
        ProjectionApiClient projectionClient)
    {
        _aggregationClient = aggregationClient;
        _projectionClient = projectionClient;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PostAsync("/api/appointments", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateAppointmentCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PutAsync("/api/appointments", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeleteAppointmentCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.DeleteAsync("/api/appointments", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _projectionClient.GetAsync($"/api/read/appointments/{id}" + Request.QueryString, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] AppointmentSearchRequest filter, CancellationToken ct)
    {
        var result = await _projectionClient.PostAsync("/api/read/appointments/search", filter, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }
}
