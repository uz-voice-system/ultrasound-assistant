using UltrasoundAssistant.Contracts.Events.AppointmentEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Appointments;

public sealed class AppointmentUpdatedEventHandler : IIntegrationEventHandler
{
    private readonly IAppointmentReadRepository _repository;

    public string RoutingKey => "appointment.updated";

    public AppointmentUpdatedEventHandler(IAppointmentReadRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = EventPayloadReader.Read<AppointmentUpdatedEvent>(payload, nameof(AppointmentUpdatedEvent));

        var appointment = await _repository.GetByIdForUpdateAsync(
            @event.AppointmentId,
            cancellationToken);

        if (appointment is null)
            return;

        if (@event.Version <= appointment.Version)
            return;

        appointment.PatientId = @event.PatientId;
        appointment.DoctorId = @event.DoctorId;
        appointment.TemplateId = @event.TemplateId;
        appointment.StartAtUtc = @event.StartAtUtc;
        appointment.EndAtUtc = @event.EndAtUtc;
        appointment.Status = @event.Status;
        appointment.Comment = @event.Comment;
        appointment.IsDeleted = false;
        appointment.UpdatedAtUtc = @event.UpdatedAtUtc;
        appointment.Version = @event.Version;

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
