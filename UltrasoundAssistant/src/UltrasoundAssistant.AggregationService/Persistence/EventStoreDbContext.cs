using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.AggregationService.Persistence.Entities;

namespace UltrasoundAssistant.AggregationService.Persistence;

public sealed class EventStoreDbContext(DbContextOptions<EventStoreDbContext> options) : DbContext(options)
{
    public DbSet<StoredEventEntity> StoredEvents => Set<StoredEventEntity>();

    public DbSet<ProcessedCommandEntity> ProcessedCommands => Set<ProcessedCommandEntity>();

    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredEventEntity>(e =>
        {
            e.ToTable("events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.EventId).IsRequired();
            e.Property(x => x.CommandId).IsRequired();
            e.Property(x => x.AggregateType).IsRequired().HasMaxLength(128);
            e.Property(x => x.AggregateId).IsRequired();
            e.Property(x => x.Version).IsRequired();
            e.Property(x => x.EventType).IsRequired().HasMaxLength(256);
            e.Property(x => x.Payload).IsRequired().HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).IsRequired();

            e.HasIndex(x => x.EventId).IsUnique();
            e.HasIndex(x => new { x.AggregateType, x.AggregateId, x.Version }).IsUnique();
            e.HasIndex(x => new { x.CommandId, x.EventType }).IsUnique();
        });

        modelBuilder.Entity<ProcessedCommandEntity>(e =>
        {
            e.ToTable("processed_commands");
            e.HasKey(x => x.CommandId);
            e.Property(x => x.CommandType).IsRequired().HasMaxLength(256);
            e.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<OutboxMessageEntity>(e =>
        {
            e.ToTable("outbox_messages");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.EventId).IsRequired();
            e.Property(x => x.ExchangeName).IsRequired().HasMaxLength(256);
            e.Property(x => x.RoutingKey).IsRequired().HasMaxLength(256);
            e.Property(x => x.EventType).IsRequired().HasMaxLength(256);
            e.Property(x => x.Payload).IsRequired().HasColumnType("jsonb");
            e.Property(x => x.Attempts).IsRequired();
            e.Property(x => x.NextAttemptAtUtc).IsRequired();
            e.Property(x => x.CreatedAtUtc).IsRequired();

            e.HasIndex(x => x.EventId).IsUnique();
            e.HasIndex(x => new { x.PublishedAtUtc, x.NextAttemptAtUtc, x.Id });
        });
    }
}
