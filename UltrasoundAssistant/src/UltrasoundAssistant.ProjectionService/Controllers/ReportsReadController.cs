using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Reports;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/reports")]
public sealed class ReportsReadController(ProjectionDbContext db) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReportDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var row = await (
            from report in db.Reports.AsNoTracking()
            join patient in db.Patients.AsNoTracking() on report.PatientId equals patient.Id into patientJoin
            from patient in patientJoin.DefaultIfEmpty()
            join template in db.Templates.AsNoTracking() on report.TemplateId equals template.Id into templateJoin
            from template in templateJoin.DefaultIfEmpty()
            where report.Id == id && !report.IsDeleted
            select new
            {
                Report = report,
                PatientName = patient != null && !patient.IsDeleted ? patient.FullName : null,
                TemplateName = template != null && !template.IsDeleted ? template.Name : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return NotFound();
        }

        return Ok(Map(row.Report, row.PatientName, row.TemplateName));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReportDto>>> List(CancellationToken cancellationToken)
    {
        var rows = await (
            from report in db.Reports.AsNoTracking()
            join patient in db.Patients.AsNoTracking() on report.PatientId equals patient.Id into patientJoin
            from patient in patientJoin.DefaultIfEmpty()
            join template in db.Templates.AsNoTracking() on report.TemplateId equals template.Id into templateJoin
            from template in templateJoin.DefaultIfEmpty()
            where !report.IsDeleted
            orderby report.UpdatedAtUtc descending
            select new
            {
                Report = report,
                PatientName = patient != null && !patient.IsDeleted ? patient.FullName : null,
                TemplateName = template != null && !template.IsDeleted ? template.Name : null
            })
            .ToListAsync(cancellationToken);

        return Ok(rows.Select(x => Map(x.Report, x.PatientName, x.TemplateName)).ToList());
    }

    private static ReportDto Map(
        Infrastructure.Persistence.Entities.ReportReadModel row,
        string? patientName,
        string? templateName)
    {
        var content = string.IsNullOrWhiteSpace(row.ContentJson)
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(row.ContentJson, JsonOptions)
              ?? new Dictionary<string, string>();

        return new ReportDto
        {
            Id = row.Id,
            Status = row.Status,
            PatientId = row.PatientId,
            PatientName = patientName,
            TemplateId = row.TemplateId,
            TemplateName = templateName,
            Content = content,
            CreatedAt = row.CreatedAtUtc,
            UpdatedAt = row.UpdatedAtUtc,
            Version = row.Version
        };
    }
}