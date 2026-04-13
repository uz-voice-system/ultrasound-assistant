using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;

namespace UltrasoundAssistant.AggregationService.Application.Common;

public abstract class CommandHandlerBase
{
    private readonly ICommandDeduplicationStore _deduplicationStore;
    private readonly IEventStore _eventStore;
    private readonly IIntegrationEventPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    protected CommandHandlerBase(
        ICommandDeduplicationStore deduplicationStore,
        IEventStore eventStore,
        IIntegrationEventPublisher publisher,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _deduplicationStore = deduplicationStore;
        _eventStore = eventStore;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    protected async Task<CommandResult> SaveAndPublishAsync(
        Guid commandId,
        string aggregateType,
        Guid aggregateId,
        int expectedVersion,
        IReadOnlyList<EventRecord> events,
        CancellationToken ct)
    {
        if (commandId == Guid.Empty)
            return CommandResult.BadRequest("CommandId is required");

        if (await _deduplicationStore.HasCommandAsync(commandId, ct))
            return CommandResult.Ok("Command already processed");

        try
        {
            await _unitOfWork.BeginAsync(ct);

            await _eventStore.AppendEventsAsync(
                aggregateType,
                aggregateId,
                expectedVersion,
                events,
                ct);

            await _publisher.PublishAsync(
                aggregateType,
                aggregateId,
                events,
                ct);

            await _deduplicationStore.MarkProcessedAsync(commandId, ct);
            await _unitOfWork.CommitAsync(ct);

            return CommandResult.Accepted($"Accepted: {events[0].EventType}");
        }
        catch (InvalidOperationException ex)
        {
            await _unitOfWork.RollbackAsync(ct);
            _logger.LogWarning(ex, "Concurrency conflict for {AggregateType}/{AggregateId}", aggregateType, aggregateId);
            return CommandResult.Conflict("Concurrency conflict while writing events");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(ct);
            _logger.LogError(ex, "Unexpected error while handling command {CommandId}", commandId);
            return CommandResult.ServerError("Unexpected write error");
        }
    }
}