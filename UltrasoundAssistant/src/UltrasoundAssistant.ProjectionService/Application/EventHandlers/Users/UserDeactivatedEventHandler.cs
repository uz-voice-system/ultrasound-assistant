using UltrasoundAssistant.Contracts.Events.UserEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Users;

public sealed class UserDeactivatedEventHandler : IIntegrationEventHandler
{
    private readonly IUserReadRepository _repository;

    public string RoutingKey => "user.deactivated";

    public UserDeactivatedEventHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = EventPayloadReader.Read<UserDeactivatedEvent>(payload, nameof(UserDeactivatedEvent));

        var user = await _repository.GetByIdForUpdateAsync(@event.UserId, cancellationToken);

        if (user is null)
            return;

        if (@event.Version <= user.Version)
            return;

        user.IsActive = false;
        user.Version = @event.Version;

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
