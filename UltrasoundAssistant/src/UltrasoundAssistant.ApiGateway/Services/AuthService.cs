using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UltrasoundAssistant.ApiGateway.Options;
using UltrasoundAssistant.Contracts.Auth;

namespace UltrasoundAssistant.ApiGateway.Services;

public sealed class AuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ProjectionApiClient _projectionClient;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        ProjectionApiClient projectionClient,
        IOptions<JwtOptions> jwtOptions)
    {
        _projectionClient = projectionClient;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<LoginResult?> LoginAsync(
        string login,
        string password,
        CancellationToken cancellationToken)
    {
        var request = new LoginRequest
        {
            Login = login,
            Password = password
        };

        var response = await _projectionClient.PostAsync(
            "/api/read/auth/verify",
            request,
            cancellationToken);

        if (response.StatusCode == StatusCodes.Status401Unauthorized)
            return null;

        if (response.StatusCode < 200 || response.StatusCode >= 300)
            throw new InvalidOperationException("Auth read service returned an unexpected response");

        var user = JsonSerializer.Deserialize<AuthUserDto>(
            response.Content,
            JsonOptions);

        if (user is null)
            throw new InvalidOperationException("Invalid AuthUserDto response");

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        return new LoginResult
        {
            UserId = user.UserId,
            Login = user.Login,
            FullName = user.FullName,
            Role = user.Role,
            Token = CreateToken(user, expiresAtUtc),
            ExpiresAtUtc = expiresAtUtc
        };
    }

    private string CreateToken(AuthUserDto user, DateTime expiresAtUtc)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Login),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("full_name", user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
