using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Auth;
using UltrasoundAssistant.ProjectionService.Persistence;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/users")]
public sealed class UsersReadController(ReadDbContext db) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var row = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (row is null)
        {
            return NotFound();
        }

        return Ok(new UserDto
        {
            Id = row.Id,
            FullName = row.Login,
            Role = row.Role,
            IsActive = row.IsActive,
            Version = row.Version
        });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> List(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = db.Users.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(u => u.IsActive);
        }

        var rows = await query
            .OrderBy(u => u.Login)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.Login,
                Role = u.Role,
                IsActive = u.IsActive,
                Version = u.Version
            })
            .ToListAsync(cancellationToken);
        return Ok(rows);
    }
}
