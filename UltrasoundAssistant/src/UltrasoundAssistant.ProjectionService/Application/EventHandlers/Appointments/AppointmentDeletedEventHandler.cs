using UltrasoundAssistant.Contracts.Events.AppointmentEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Appointments;

public sealed class AppointmentDeletedEventHandler : IIntegrationEventHandler
{
    private readonly IAppointmentReadRepository _repository;

    public string RoutingKey => "appointment.deleted";

    public AppointmentDeletedEventHandler(IAppointmentReadRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = EventPayloadReader.Read<AppointmentDeletedEvent>(payload, nameof(AppointmentDeletedEvent));

        var appointment = await _repository.GetByIdForUpdateAsync(
            @event.AppointmentId,
            cancellationToken);

        if (appointment is null)
            return;

        if (@event.Version <= appointment.Version)
            return;

        appointment.IsDeleted = true;
        appointment.UpdatedAtUtc = @event.UpdatedAtUtc;
        appointment.Version = @event.Version;

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
