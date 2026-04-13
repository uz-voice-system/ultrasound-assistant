using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.AggregationService.Infrastructure.Persistence;

public sealed class EfCoreCommandDeduplicationStore : ICommandDeduplicationStore
{
    private readonly AggregationDbContext _dbContext;

    public EfCoreCommandDeduplicationStore(AggregationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasCommandAsync(Guid commandId, CancellationToken cancellationToken)
    {
        return await _dbContext.ProcessedCommands
            .AsNoTracking()
            .AnyAsync(x => x.CommandId == commandId, cancellationToken);
    }

    public async Task MarkProcessedAsync(Guid commandId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ProcessedCommands
            .AnyAsync(x => x.CommandId == commandId, cancellationToken);

        if (exists)
            return;

        _dbContext.ProcessedCommands.Add(new ProcessedCommandEntity
        {
            CommandId = commandId,
            ProcessedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}