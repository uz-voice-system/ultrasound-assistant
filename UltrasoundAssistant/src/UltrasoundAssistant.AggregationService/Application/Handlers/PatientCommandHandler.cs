using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Application.Common;
using UltrasoundAssistant.AggregationService.Application.Validation;
using UltrasoundAssistant.AggregationService.Domain;
using UltrasoundAssistant.Contracts.Commands.Patients;

namespace UltrasoundAssistant.AggregationService.Application.Handlers;

public sealed class PatientCommandHandler : CommandHandlerBase
{
    private const string AggregateType = "patient";
    private const string PatientCreatedRoutingKey = "patient.created";
    private const string PatientUpdatedRoutingKey = "patient.updated";
    private const string PatientDeletedRoutingKey = "patient.deleted";

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

            var aggregate = await LoadAggregateAsync(command.PatientId, ct);

            var @event = aggregate.Create(
                command.PatientId,
                command.FullName,
                command.BirthDate,
                command.Gender,
                command.PhoneNumber,
                command.Email,
                command.Documents);

            return await SaveAndPublishAsync(
                AggregateType,
                command.PatientId,
                aggregate.Version,
                [EventFactory.Create(@event, PatientCreatedRoutingKey)],
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

            var aggregate = await LoadAggregateAsync(command.PatientId, ct);

            if (!aggregate.Exists || aggregate.IsDeleted)
                return CommandResult.NotFound("Patient not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict(
                    $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.Update(
                command.FullName,
                command.BirthDate,
                command.Gender,
                command.PhoneNumber,
                command.Email,
                command.Documents);

            return await SaveAndPublishAsync(
                AggregateType,
                command.PatientId,
                aggregate.Version,
                [EventFactory.Create(@event, PatientUpdatedRoutingKey)],
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

    public async Task<CommandResult> DeleteAsync(DeletePatientCommand command, CancellationToken ct)
    {
        try
        {
            PatientCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.PatientId, ct);

            if (!aggregate.Exists || aggregate.IsDeleted)
                return CommandResult.NotFound("Patient not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict(
                    $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.Delete();

            return await SaveAndPublishAsync(
                AggregateType,
                command.PatientId,
                aggregate.Version,
                [EventFactory.Create(@event, PatientDeletedRoutingKey)],
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

    private async Task<PatientAggregate> LoadAggregateAsync(Guid patientId, CancellationToken ct)
    {
        var history = await _eventStore.LoadAggregateEventsAsync(AggregateType, patientId, ct);

        var aggregate = new PatientAggregate();
        aggregate.LoadFrom(history);

        return aggregate;
    }
}
