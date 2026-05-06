using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Reads.Users.Details;
using UltrasoundAssistant.Contracts.Reads.Users.Search;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/users")]
public sealed class UsersReadController : ControllerBase
{
    private readonly IUserReadService _userReadService;

    public UsersReadController(IUserReadService userReadService)
    {
        _userReadService = userReadService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, [FromQuery] bool includeInactive, CancellationToken cancellationToken)
    {
        var user = await _userReadService.GetByIdAsync(id, includeInactive, cancellationToken);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<UserSummaryDto>>> Search([FromBody] UserSearchRequest filter, CancellationToken cancellationToken)
    {
        var users = await _userReadService.SearchAsync(filter, cancellationToken);

        return Ok(users);
    }
}
