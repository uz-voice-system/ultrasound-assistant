using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UltrasoundAssistant.AggregationService.Persistence;

/// <summary>
/// Фабрика для dotnet ef (миграции без запуска приложения).
/// </summary>
public sealed class EventStoreDbContextFactory : IDesignTimeDbContextFactory<EventStoreDbContext>
{
    public EventStoreDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EventStoreDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5433;Database=event_store;Username=event_user;Password=event_pass");
        return new EventStoreDbContext(optionsBuilder.Options);
    }
}
