using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Application.Common;
using UltrasoundAssistant.AggregationService.Application.Validation;
using UltrasoundAssistant.AggregationService.Domain;
using UltrasoundAssistant.Contracts.Commands.Appointments;

namespace UltrasoundAssistant.AggregationService.Application.Handlers;

public sealed class AppointmentCommandHandler : CommandHandlerBase
{
    private const string AggregateType = "appointment";
    private const string AppointmentCreatedRoutingKey = "appointment.created";
    private const string AppointmentUpdatedRoutingKey = "appointment.updated";
    private const string AppointmentDeletedRoutingKey = "appointment.deleted";

    private readonly IEventStore _eventStore;

    public AppointmentCommandHandler(
        IEventStore eventStore,
        IIntegrationEventPublisher publisher,
        IUnitOfWork unitOfWork,
        ILogger<AppointmentCommandHandler> logger)
        : base(eventStore, publisher, unitOfWork, logger)
    {
        _eventStore = eventStore;
    }

    public async Task<CommandResult> CreateAsync(CreateAppointmentCommand command, CancellationToken ct)
    {
        try
        {
            AppointmentCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.AppointmentId, ct);

            var @event = aggregate.Create(
                command.AppointmentId,
                command.PatientId,
                command.DoctorId,
                command.TemplateId,
                command.CreatedByUserId,
                command.StartAtUtc,
                command.EndAtUtc,
                command.Comment);

            return await SaveAndPublishAsync(
                AggregateType,
                command.AppointmentId,
                aggregate.Version,
                [EventFactory.Create(@event, AppointmentCreatedRoutingKey)],
                ct);
        }
        catch (ArgumentException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
    }

    public async Task<CommandResult> UpdateAsync(UpdateAppointmentCommand command, CancellationToken ct)
    {
        try
        {
            AppointmentCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.AppointmentId, ct);

            if (!aggregate.Exists || aggregate.IsDeleted)
                return CommandResult.NotFound("Appointment not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict(
                    $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.Update(
                command.PatientId,
                command.DoctorId,
                command.TemplateId,
                command.StartAtUtc,
                command.EndAtUtc,
                command.Status,
                command.Comment);

            return await SaveAndPublishAsync(
                AggregateType,
                command.AppointmentId,
                aggregate.Version,
                [EventFactory.Create(@event, AppointmentUpdatedRoutingKey)],
                ct);
        }
        catch (ArgumentException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
    }

    public async Task<CommandResult> DeleteAsync(DeleteAppointmentCommand command, CancellationToken ct)
    {
        try
        {
            AppointmentCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.AppointmentId, ct);

            if (!aggregate.Exists || aggregate.IsDeleted)
                return CommandResult.NotFound("Appointment not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict(
                    $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.Delete();

            return await SaveAndPublishAsync(
                AggregateType,
                command.AppointmentId,
                aggregate.Version,
                [EventFactory.Create(@event, AppointmentDeletedRoutingKey)],
                ct);
        }
        catch (ArgumentException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
    }

    private async Task<AppointmentAggregate> LoadAggregateAsync(Guid appointmentId, CancellationToken ct)
    {
        var history = await _eventStore.LoadAggregateEventsAsync(AggregateType, appointmentId, ct);

        var aggregate = new AppointmentAggregate();
        aggregate.LoadFrom(history);

        return aggregate;
    }
}
