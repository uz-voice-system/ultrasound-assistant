namespace UltrasoundAssistant.AggregationService.Application.Abstractions;

public interface ICommandDeduplicationStore
{
    Task<bool> HasCommandAsync(Guid commandId, CancellationToken cancellationToken);
    Task MarkProcessedAsync(Guid commandId, CancellationToken cancellationToken);
}