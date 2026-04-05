using UltrasoundAssistant.AggregationService.Infrastructure;
using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.UserEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class UserAggregate
{
    public Guid Id { get; private set; }
    public bool Exists { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Version { get; private set; }
    public string Login { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }

    public void LoadFrom(IEnumerable<EventRecordEnvelope> events)
    {
        foreach (var item in events)
        {
            Apply(item.EventType, item.Payload, item.Version);
        }
    }

    public UserCreatedEvent Create(Guid userId, string login, string passwordHash, UserRole role)
    {
        if (Exists)
        {
            throw new DomainException("User already exists");
        }

        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Invalid user data");
        }

        return new UserCreatedEvent
        {
            Id = userId,
            Login = login.Trim(),
            PasswordHash = passwordHash,
            Role = role,
            Version = Version + 1
        };
    }

    public UserUpdatedEvent Update(string? newLogin, string? newPasswordHash, UserRole? newRole)
    {
        if (!Exists || !IsActive)
        {
            throw new DomainException("User not found or inactive");
        }

        if (newLogin is null && newPasswordHash is null && newRole is null)
        {
            throw new DomainException("At least one field must be provided");
        }

        var login = newLogin is null ? Login : newLogin.Trim();
        var hash = newPasswordHash ?? PasswordHash;
        var role = newRole ?? Role;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(hash))
        {
            throw new DomainException("Login and password hash are required");
        }

        return new UserUpdatedEvent
        {
            UserId = Id,
            Login = login,
            PasswordHash = hash,
            Role = role,
            Version = Version + 1
        };
    }

    public UserDeactivatedEvent Deactivate(Guid userId)
    {
        if (!Exists || userId != Id)
        {
            throw new DomainException("User not found");
        }

        if (!IsActive)
        {
            throw new DomainException("User already deactivated");
        }

        return new UserDeactivatedEvent
        {
            UserId = userId,
            Version = Version + 1
        };
    }

    private void Apply(string eventType, string payload, int version)
    {
        switch (eventType)
        {
            case nameof(UserCreatedEvent):
                var created = System.Text.Json.JsonSerializer.Deserialize<UserCreatedEvent>(payload)
                    ?? throw new DomainException("Failed to deserialize UserCreatedEvent");
                Id = created.Id;
                Exists = true;
                IsActive = true;
                Login = created.Login;
                PasswordHash = created.PasswordHash;
                Role = created.Role;
                Version = version;
                break;
            case nameof(UserUpdatedEvent):
                var updated = System.Text.Json.JsonSerializer.Deserialize<UserUpdatedEvent>(payload)
                    ?? throw new DomainException("Failed to deserialize UserUpdatedEvent");
                Id = updated.UserId;
                Exists = true;
                Login = updated.Login;
                PasswordHash = updated.PasswordHash;
                Role = updated.Role;
                Version = version;
                break;
            case nameof(UserDeactivatedEvent):
                IsActive = false;
                Version = version;
                break;
        }
    }
}
