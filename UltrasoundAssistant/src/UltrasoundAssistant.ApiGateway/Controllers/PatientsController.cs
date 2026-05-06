using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.ApiGateway.Services;
using UltrasoundAssistant.Contracts.Commands.Patients;
using UltrasoundAssistant.Contracts.Reads.Patients.Search;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[Route("api/patients")]
public sealed class PatientsController : GatewayControllerBase
{
    private readonly AggregationApiClient _aggregationClient;
    private readonly ProjectionApiClient _projectionClient;

    public PatientsController(
        AggregationApiClient aggregationClient,
        ProjectionApiClient projectionClient)
    {
        _aggregationClient = aggregationClient;
        _projectionClient = projectionClient;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PostAsync("/api/patients", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdatePatientCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.PutAsync("/api/patients", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeletePatientCommand command, CancellationToken ct)
    {
        var result = await _aggregationClient.DeleteAsync("/api/patients", command, ct);
        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _projectionClient.GetAsync($"/api/read/patients/{id}" + Request.QueryString, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] PatientSearchRequest filter, CancellationToken ct)
    {
        var result = await _projectionClient.PostAsync("/api/read/patients/search", filter, ct);

        return ProxyJson(result.StatusCode, result.Content);
    }
}
