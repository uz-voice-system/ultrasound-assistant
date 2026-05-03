using UltrasoundAssistant.Contracts.Entity.Users;
using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.UserEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Users;

public sealed class UserCreatedEventHandler : IIntegrationEventHandler
{
    private readonly IUserReadRepository _repository;

    public string RoutingKey => "user.created";

    public UserCreatedEventHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = EventPayloadReader.Read<UserCreatedEvent>(payload, nameof(UserCreatedEvent));

        var user = await _repository.GetByIdForUpdateAsync(@event.UserId, cancellationToken);

        if (user is not null && @event.Version <= user.Version)
            return;

        if (user is null)
        {
            user = new UserReadModel
            {
                Id = @event.UserId
            };

            await _repository.AddAsync(user, cancellationToken);
        }

        user.Login = @event.Login;
        user.PasswordHash = @event.PasswordHash;
        user.FullName = @event.FullName;
        user.Role = @event.Role;
        user.IsActive = true;
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
