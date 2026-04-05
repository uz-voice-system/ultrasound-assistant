using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Patients;
using UltrasoundAssistant.ProjectionService.Persistence;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/patients")]
public sealed class PatientsReadController(ReadDbContext db) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PatientDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var row = await db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
        if (row is null)
        {
            return NotFound();
        }

        return Ok(new PatientDto
        {
            Id = row.Id,
            FullName = row.FullName,
            BirthDate = row.BirthDate.ToDateTime(TimeOnly.MinValue),
            Gender = row.Gender,
            Version = row.Version
        });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PatientDto>>> List(CancellationToken cancellationToken)
    {
        var list = await db.Patients.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.FullName)
            .Select(p => new PatientDto
            {
                Id = p.Id,
                FullName = p.FullName,
                BirthDate = p.BirthDate.ToDateTime(TimeOnly.MinValue),
                Gender = p.Gender,
                Version = p.Version
            })
            .ToListAsync(cancellationToken);
        return Ok(list);
    }
}
