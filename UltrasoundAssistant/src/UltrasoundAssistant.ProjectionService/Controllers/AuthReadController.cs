using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Auth;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/auth")]
public sealed class AuthReadController : ControllerBase
{
    private readonly IAuthReadService _authReadService;

    public AuthReadController(IAuthReadService authReadService)
    {
        _authReadService = authReadService;
    }

    [HttpPost("verify")]
    public async Task<ActionResult<AuthUserDto>> VerifyCredentials([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _authReadService.VerifyCredentialsAsync(request, cancellationToken);

        if (user is null)
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(user);
    }
}
