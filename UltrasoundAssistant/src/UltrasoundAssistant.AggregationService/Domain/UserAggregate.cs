using System.Text.Json;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;
using UltrasoundAssistant.Contracts.Entity.Users;
using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.UserEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class UserAggregate
{
    public Guid Id { get; private set; }

    public bool Exists { get; private set; }

    public bool IsActive { get; private set; }

    public int Version { get; private set; }

    public string Login { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public DoctorProfileDto? DoctorProfile { get; private set; }

    public UserCreatedEvent Create(
        Guid userId,
        string login,
        string passwordHash,
        string fullName,
        UserRole role,
        DoctorProfileDto? doctorProfile)
    {
        if (Exists)
            throw new DomainException("User already exists");

        return new UserCreatedEvent
        {
            UserId = userId,
            Login = login.Trim(),
            PasswordHash = passwordHash.Trim(),
            FullName = fullName.Trim(),
            Role = role,
            DoctorProfile = CloneDoctorProfile(doctorProfile),
            Version = Version + 1
        };
    }

    public UserUpdatedEvent Update(
        string login,
        string passwordHash,
        string fullName,
        UserRole role,
        bool isActive,
        DoctorProfileDto? doctorProfile)
    {
        if (!Exists)
            throw new DomainException("User not found");

        return new UserUpdatedEvent
        {
            UserId = Id,
            Login = login.Trim(),
            PasswordHash = passwordHash.Trim(),
            FullName = fullName.Trim(),
            Role = role,
            IsActive = isActive,
            DoctorProfile = CloneDoctorProfile(doctorProfile),
            Version = Version + 1
        };
    }

    public UserActivatedEvent Activate()
    {
        if (!Exists)
            throw new DomainException("User not found");

        if (IsActive)
            throw new DomainException("User already active");

        return new UserActivatedEvent
        {
            UserId = Id,
            Version = Version + 1
        };
    }

    public UserDeactivatedEvent Deactivate()
    {
        if (!Exists)
            throw new DomainException("User not found");

        if (!IsActive)
            throw new DomainException("User already inactive");

        return new UserDeactivatedEvent
        {
            UserId = Id,
            Version = Version + 1
        };
    }

    public void LoadFrom(IEnumerable<EventRecord> history)
    {
        foreach (var item in history.OrderBy(x => x.Version))
            Apply(item);
    }

    private void Apply(EventRecord record)
    {
        switch (record.EventType)
        {
            case nameof(UserCreatedEvent):
                {
                    var e = JsonSerializer.Deserialize<UserCreatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid UserCreatedEvent payload");

                    Id = e.UserId;
                    Login = e.Login;
                    PasswordHash = e.PasswordHash;
                    FullName = e.FullName;
                    Role = e.Role;
                    DoctorProfile = CloneDoctorProfile(e.DoctorProfile);
                    Exists = true;
                    IsActive = true;
                    Version = e.Version;
                    break;
                }

            case nameof(UserUpdatedEvent):
                {
                    var e = JsonSerializer.Deserialize<UserUpdatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid UserUpdatedEvent payload");

                    Login = e.Login;
                    PasswordHash = e.PasswordHash;
                    FullName = e.FullName;
                    Role = e.Role;
                    IsActive = e.IsActive;
                    DoctorProfile = CloneDoctorProfile(e.DoctorProfile);
                    Exists = true;
                    Version = e.Version;
                    break;
                }

            case nameof(UserActivatedEvent):
                {
                    var e = JsonSerializer.Deserialize<UserActivatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid UserActivatedEvent payload");

                    IsActive = true;
                    Version = e.Version;
                    break;
                }

            case nameof(UserDeactivatedEvent):
                {
                    var e = JsonSerializer.Deserialize<UserDeactivatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid UserDeactivatedEvent payload");

                    IsActive = false;
                    Version = e.Version;
                    break;
                }
        }
    }

    private static DoctorProfileDto? CloneDoctorProfile(DoctorProfileDto? profile)
    {
        if (profile is null)
            return null;

        return new DoctorProfileDto
        {
            Specialization = NormalizeNullable(profile.Specialization),
            Cabinet = NormalizeNullable(profile.Cabinet),
            PhoneExtension = NormalizeNullable(profile.PhoneExtension)
        };
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
