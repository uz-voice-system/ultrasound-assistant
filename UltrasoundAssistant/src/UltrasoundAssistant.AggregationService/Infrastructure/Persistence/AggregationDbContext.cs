using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.AggregationService.Infrastructure.Persistence;

public sealed class AggregationDbContext : DbContext
{
    public AggregationDbContext(DbContextOptions<AggregationDbContext> options)
        : base(options)
    {
    }

    public DbSet<EventEntity> Events => Set<EventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureEvents(modelBuilder);
    }

    private static void ConfigureEvents(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<EventEntity>();

        entity.ToTable("events");

        entity.HasKey(x => x.EventId);

        entity.Property(x => x.AggregateType)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.AggregateId)
            .IsRequired();

        entity.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.Payload)
            .IsRequired();

        entity.Property(x => x.RoutingKey)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.Version)
            .IsRequired();

        entity.Property(x => x.CreatedAtUtc)
            .IsRequired();

        entity.HasIndex(x => new { x.AggregateType, x.AggregateId, x.Version })
            .IsUnique();

        entity.HasIndex(x => new { x.AggregateType, x.AggregateId });
    }
}