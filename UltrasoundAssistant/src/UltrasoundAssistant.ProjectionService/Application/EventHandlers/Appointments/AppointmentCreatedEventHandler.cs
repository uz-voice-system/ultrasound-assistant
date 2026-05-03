using UltrasoundAssistant.Contracts.Events.AppointmentEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Appointments;

public sealed class AppointmentCreatedEventHandler : IIntegrationEventHandler
{
    private readonly IAppointmentReadRepository _repository;

    public string RoutingKey => "appointment.created";

    public AppointmentCreatedEventHandler(IAppointmentReadRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = EventPayloadReader.Read<AppointmentCreatedEvent>(payload, nameof(AppointmentCreatedEvent));

        var appointment = await _repository.GetByIdForUpdateAsync(
            @event.AppointmentId,
            cancellationToken);

        if (appointment is not null && @event.Version <= appointment.Version)
            return;

        if (appointment is null)
        {
            appointment = new AppointmentReadModel
            {
                Id = @event.AppointmentId
            };

            await _repository.AddAsync(appointment, cancellationToken);
        }

        appointment.PatientId = @event.PatientId;
        appointment.DoctorId = @event.DoctorId;
        appointment.TemplateId = @event.TemplateId;
        appointment.CreatedByUserId = @event.CreatedByUserId;
        appointment.StartAtUtc = @event.StartAtUtc;
        appointment.EndAtUtc = @event.EndAtUtc;
        appointment.Status = @event.Status;
        appointment.Comment = @event.Comment;
        appointment.IsDeleted = false;
        appointment.CreatedAtUtc = @event.CreatedAtUtc;
        appointment.UpdatedAtUtc = @event.UpdatedAtUtc;
        appointment.Version = @event.Version;

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
