using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Application.Common;
using UltrasoundAssistant.AggregationService.Application.Validation;
using UltrasoundAssistant.AggregationService.Domain;
using UltrasoundAssistant.Contracts.Commands.Schedules;

namespace UltrasoundAssistant.AggregationService.Application.Handlers;

public sealed class UserScheduleCommandHandler : CommandHandlerBase
{
    private const string AggregateType = "user_schedule";
    private const string UserScheduleUpdatedRoutingKey = "user_schedule.updated";

    private readonly IEventStore _eventStore;

    public UserScheduleCommandHandler(
        IEventStore eventStore,
        IIntegrationEventPublisher publisher,
        IUnitOfWork unitOfWork,
        ILogger<UserScheduleCommandHandler> logger)
        : base(eventStore, publisher, unitOfWork, logger)
    {
        _eventStore = eventStore;
    }

    public async Task<CommandResult> UpdateAsync(
        UpdateUserScheduleCommand command,
        CancellationToken ct)
    {
        try
        {
            UserScheduleCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.UserId, ct);

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict(
                    $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.Update(
                command.UserId,
                command.Items);

            return await SaveAndPublishAsync(
                AggregateType,
                command.UserId,
                aggregate.Version,
                [EventFactory.Create(@event, UserScheduleUpdatedRoutingKey)],
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

    private async Task<UserScheduleAggregate> LoadAggregateAsync(
        Guid userId,
        CancellationToken ct)
    {
        var history = await _eventStore.LoadAggregateEventsAsync(
            AggregateType,
            userId,
            ct);

        var aggregate = new UserScheduleAggregate();
        aggregate.LoadFrom(history);

        return aggregate;
    }
}
