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
    public string Role { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}