using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.ApiGateway.Services;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Login and password are required" });

        var result = _authService.Login(request.Login, request.Password);
        if (result is null)
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(result);
    }
}

public sealed class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}