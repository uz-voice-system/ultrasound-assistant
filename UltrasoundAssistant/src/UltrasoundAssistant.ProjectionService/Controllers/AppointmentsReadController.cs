using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Reads.Appointments.Details;
using UltrasoundAssistant.Contracts.Reads.Appointments.Search;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/appointments")]
public sealed class AppointmentsReadController : ControllerBase
{
    private readonly IAppointmentReadService _appointmentReadService;

    public AppointmentsReadController(IAppointmentReadService appointmentReadService)
    {
        _appointmentReadService = appointmentReadService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentDto>> GetById(
        Guid id,
        [FromQuery] bool includeDeleted,
        CancellationToken cancellationToken)
    {
        var appointment = await _appointmentReadService.GetByIdAsync(
            id,
            includeDeleted,
            cancellationToken);

        if (appointment is null)
            return NotFound();

        return Ok(appointment);
    }

    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<AppointmentSummaryDto>>> Search(
        [FromBody] AppointmentSearchRequest filter,
        CancellationToken cancellationToken)
    {
        var appointments = await _appointmentReadService.SearchAsync(
            filter,
            cancellationToken);

        return Ok(appointments);
    }
}
