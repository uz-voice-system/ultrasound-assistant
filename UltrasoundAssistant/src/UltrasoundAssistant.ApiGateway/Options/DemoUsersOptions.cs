using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.ApiGateway.Options;

public sealed class DemoUsersOptions
{
    public List<DemoUser> Users { get; set; } = [];
}

public sealed class DemoUser
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string FullName { get; set; } = string.Empty;
}