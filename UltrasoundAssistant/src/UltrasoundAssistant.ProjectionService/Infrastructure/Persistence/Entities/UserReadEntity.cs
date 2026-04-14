using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

public sealed class UserReadEntity
{
    public Guid Id { get; set; }

    public string Login { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public int Version { get; set; }
}
