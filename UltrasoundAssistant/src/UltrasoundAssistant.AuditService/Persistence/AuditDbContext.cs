using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.AuditService.Persistence.Entities;

namespace UltrasoundAssistant.AuditService.Persistence;

public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogEntity>(e =>
        {
            e.ToTable("audit_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.ActionType).IsRequired().HasMaxLength(128);
            e.Property(x => x.DetailsJson).IsRequired().HasColumnName("details").HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).IsRequired();
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.ActionType);
            e.HasIndex(x => x.CreatedAt);
        });
    }
}
