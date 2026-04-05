using Microsoft.EntityFrameworkCore;

namespace UltrasoundAssistant.ProjectionService.Persistence;

public sealed class MigrationHostedService(IServiceProvider serviceProvider, ILogger<MigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Read DB migrations applied");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
