using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Templates;
using UltrasoundAssistant.ProjectionService.Persistence;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/templates")]
public sealed class TemplatesReadController(ReadDbContext db) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TemplateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var row = await db.Templates.AsNoTracking()
            .Include(t => t.Keywords)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
        if (row is null)
        {
            return NotFound();
        }

        return Ok(Map(row));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TemplateDto>>> List(CancellationToken cancellationToken)
    {
        var rows = await db.Templates.AsNoTracking()
            .Include(t => t.Keywords)
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(Map).ToList());
    }

    private static TemplateDto Map(Persistence.Entities.TemplateReadEntity row) =>
        new()
        {
            Id = row.Id,
            Name = row.Name,
            Version = row.Version,
            Keywords = row.Keywords
                .OrderBy(k => k.Phrase)
                .Select(k => new TemplateKeywordDto { Id = k.Id, Phrase = k.Phrase, TargetField = k.TargetField })
                .ToList()
        };
}
