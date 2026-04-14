using Microsoft.Extensions.Options;
using UltrasoundAssistant.ApiGateway.Options;

namespace UltrasoundAssistant.ApiGateway.Services;

public sealed class AuthService
{
    private readonly DemoUsersOptions _options;

    public AuthService(IOptions<DemoUsersOptions> options)
    {
        _options = options.Value;
    }

    public LoginResult? Login(string login, string password)
    {
        var user = _options.Users.FirstOrDefault(x =>
            string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase) &&
            x.Password == password);

        if (user is null)
            return null;

        return new LoginResult
        {
            UserId = user.Id,
            Login = user.Login,
            FullName = user.FullName,
            Role = user.Role,
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
        };
    }
}

public sealed class LoginResult
{
    public Guid UserId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}