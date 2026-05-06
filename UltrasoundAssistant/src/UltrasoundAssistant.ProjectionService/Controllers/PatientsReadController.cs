using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Reads.Patients.Details;
using UltrasoundAssistant.Contracts.Reads.Patients.Search;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/patients")]
public sealed class PatientsReadController : ControllerBase
{
    private readonly IPatientReadService _patientReadService;

    public PatientsReadController(IPatientReadService patientReadService)
    {
        _patientReadService = patientReadService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PatientDto>> GetById(Guid id, [FromQuery] bool includeDeleted, CancellationToken cancellationToken)
    {
        var patient = await _patientReadService.GetByIdAsync(id, includeDeleted, cancellationToken);

        if (patient is null)
            return NotFound();

        return Ok(patient);
    }

    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<PatientSummaryDto>>> Search([FromBody] PatientSearchRequest filter, CancellationToken cancellationToken)
    {
        var patients = await _patientReadService.SearchAsync(filter, cancellationToken);

        return Ok(patients);
    }
}
