using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Reports;
using UltrasoundAssistant.ProjectionService.Persistence;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/reports")]
public sealed class ReportsReadController(ReadDbContext db) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReportDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var row = await db.Reports.AsNoTracking()
            .Include(r => r.Patient)
            .Include(r => r.Template)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
        if (row is null)
        {
            return NotFound();
        }

        return Ok(Map(row));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReportDto>>> List(CancellationToken cancellationToken)
    {
        var rows = await db.Reports.AsNoTracking()
            .Include(r => r.Patient)
            .Include(r => r.Template)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(Map).ToList());
    }

    private static ReportDto Map(Persistence.Entities.ReportReadEntity row)
    {
        var content = string.IsNullOrWhiteSpace(row.ContentJson)
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(row.ContentJson, JsonOptions) ?? [];

        return new ReportDto
        {
            Id = row.Id,
            Status = row.Status,
            PatientId = row.PatientId,
            PatientName = row.Patient?.FullName,
            TemplateId = row.TemplateId,
            TemplateName = row.Template?.Name,
            Content = content,
            CreatedAt = row.CreatedAt.UtcDateTime,
            UpdatedAt = row.UpdatedAt.UtcDateTime,
            Version = row.Version
        };
    }
}
