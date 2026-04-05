using Microsoft.EntityFrameworkCore;

namespace UltrasoundAssistant.AggregationService.Persistence;

public sealed class MigrationHostedService(IServiceProvider serviceProvider, ILogger<MigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Event Store database migrations applied");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
