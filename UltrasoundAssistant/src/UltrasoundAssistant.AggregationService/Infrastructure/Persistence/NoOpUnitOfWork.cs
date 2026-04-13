using UltrasoundAssistant.AggregationService.Application.Abstractions;

namespace UltrasoundAssistant.AggregationService.Infrastructure.Persistence;

public sealed class NoOpUnitOfWork : IUnitOfWork
{
    public Task BeginAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}