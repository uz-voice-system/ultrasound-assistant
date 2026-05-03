using UltrasoundAssistant.Contracts.Entity.Users;
using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.UserEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Users;

public sealed class UserUpdatedEventHandler : IIntegrationEventHandler
{
    private readonly IUserReadRepository _repository;

    public string RoutingKey => "user.updated";

    public UserUpdatedEventHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = EventPayloadReader.Read<UserUpdatedEvent>(payload, nameof(UserUpdatedEvent));

        var user = await _repository.GetByIdForUpdateAsync(@event.UserId, cancellationToken);

        if (user is null)
            return;

        if (@event.Version <= user.Version)
            return;

        user.Login = @event.Login;
        user.PasswordHash = @event.PasswordHash;
        user.FullName = @event.FullName;
        user.Role = @event.Role;
        user.IsActive = @event.IsActive;
        user.Version = @event.Version;

        SyncDoctorProfile(user, @event.DoctorProfile);

        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static void SyncDoctorProfile(UserReadModel user, DoctorProfileDto? profile)
    {
        if (user.Role != UserRole.Doctor)
        {
            user.DoctorProfile = null;
            return;
        }

        user.DoctorProfile ??= new DoctorProfileReadModel
        {
            UserId = user.Id
        };

        user.DoctorProfile.Specialization = profile?.Specialization;
        user.DoctorProfile.Cabinet = profile?.Cabinet;
        user.DoctorProfile.PhoneExtension = profile?.PhoneExtension;
    }
}
