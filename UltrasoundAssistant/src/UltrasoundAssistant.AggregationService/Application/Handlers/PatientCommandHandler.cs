using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Application.Common;
using UltrasoundAssistant.AggregationService.Application.Validation;
using UltrasoundAssistant.AggregationService.Domain;
using UltrasoundAssistant.Contracts.Commands.Patients;

namespace UltrasoundAssistant.AggregationService.Application.Handlers;

public sealed class PatientCommandHandler : CommandHandlerBase
{
    private readonly IEventStore _eventStore;

    public PatientCommandHandler(
        IEventStore eventStore,
        IIntegrationEventPublisher publisher,
        IUnitOfWork unitOfWork,
        ILogger<PatientCommandHandler> logger)
        : base(eventStore, publisher, unitOfWork, logger)
    {
        _eventStore = eventStore;
    }

    public async Task<CommandResult> CreateAsync(CreatePatientCommand command, CancellationToken ct)
    {
        try
        {
            PatientCommandValidator.Validate(command);

            var history = await _eventStore.LoadAggregateEventsAsync("patient", command.PatientId, ct);
            var aggregate = new PatientAggregate();
            aggregate.LoadFrom(history);

            var @event = aggregate.Create(command.PatientId, command.FullName, command.BirthDate, command.Gender);

            return await SaveAndPublishAsync(
                "patient",
                command.PatientId,
                aggregate.Version,
                [EventFactory.Create(@event, "patient.created")],
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

    public async Task<CommandResult> UpdateAsync(UpdatePatientCommand command, CancellationToken ct)
    {
        try
        {
            PatientCommandValidator.Validate(command);

            var history = await _eventStore.LoadAggregateEventsAsync("patient", command.PatientId, ct);
            var aggregate = new PatientAggregate();
            aggregate.LoadFrom(history);

            if (!aggregate.Exists)
                return CommandResult.NotFound("Patient not found");

            if (command.ExpectedVersion != aggregate.Version)
                return CommandResult.Conflict($"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");

            var @event = aggregate.Update(command.FullName, command.BirthDate, command.Gender);

            return await SaveAndPublishAsync(
                "patient",
                command.PatientId,
                aggregate.Version,
                [EventFactory.Create(@event, "patient.updated")],
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

    public async Task<CommandResult> DeactivateAsync(DeactivatePatientCommand command, CancellationToken ct)
    {
        try
        {
            PatientCommandValidator.Validate(command);

            var history = await _eventStore.LoadAggregateEventsAsync("patient", command.PatientId, ct);
            var aggregate = new PatientAggregate();
            aggregate.LoadFrom(history);

            if (!aggregate.Exists)
                return CommandResult.NotFound("Patient not found");

            if (command.ExpectedVersion != aggregate.Version)
                return CommandResult.Conflict($"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");

            var @event = aggregate.Deactivate(command.PatientId, command.Reason);

            return await SaveAndPublishAsync(
                "patient",
                command.PatientId,
                aggregate.Version,
                [EventFactory.Create(@event, "patient.deactivated")],
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
}