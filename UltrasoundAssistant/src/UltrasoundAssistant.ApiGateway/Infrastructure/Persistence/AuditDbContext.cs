using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.ApiGateway.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ApiGateway.Infrastructure.Persistence;

public sealed class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureAuditLogs(modelBuilder);
    }

    private static void ConfigureAuditLogs(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AuditLogEntity>();

        entity.ToTable("audit_logs");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.TraceId).IsRequired().HasMaxLength(100);

        entity.Property(x => x.UserLogin).HasMaxLength(100);
        entity.Property(x => x.UserRole).HasMaxLength(50);

        entity.Property(x => x.Method).IsRequired().HasMaxLength(20);
        entity.Property(x => x.Path).IsRequired().HasMaxLength(1000);
        entity.Property(x => x.QueryString).HasMaxLength(2000);
        entity.Property(x => x.Endpoint).HasMaxLength(1000);

        entity.Property(x => x.Operation).IsRequired().HasMaxLength(100);
        entity.Property(x => x.EntityType).HasMaxLength(100);

        entity.Property(x => x.StatusCode).IsRequired();
        entity.Property(x => x.Succeeded).IsRequired();

        entity.Property(x => x.ErrorMessage).HasMaxLength(4000);

        entity.Property(x => x.StartedAtUtc).IsRequired();
        entity.Property(x => x.FinishedAtUtc).IsRequired();
        entity.Property(x => x.DurationMs).IsRequired();

        entity.Property(x => x.ClientIp).HasMaxLength(100);
        entity.Property(x => x.UserAgent).HasMaxLength(1000);

        entity.Property(x => x.RequestBodyJson);

        entity.HasIndex(x => x.StartedAtUtc);
        entity.HasIndex(x => x.UserId);
        entity.HasIndex(x => x.UserRole);
        entity.HasIndex(x => x.Path);
        entity.HasIndex(x => x.Operation);
        entity.HasIndex(x => x.EntityType);
        entity.HasIndex(x => x.EntityId);
        entity.HasIndex(x => x.StatusCode);
        entity.HasIndex(x => x.Succeeded);
    }
}
