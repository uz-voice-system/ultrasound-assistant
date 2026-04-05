using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.ProjectionService.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Persistence;

public sealed class ReadDbContext(DbContextOptions<ReadDbContext> options) : DbContext(options)
{
    public DbSet<UserReadEntity> Users => Set<UserReadEntity>();

    public DbSet<PatientReadEntity> Patients => Set<PatientReadEntity>();

    public DbSet<TemplateReadEntity> Templates => Set<TemplateReadEntity>();

    public DbSet<KeywordReadEntity> Keywords => Set<KeywordReadEntity>();

    public DbSet<ReportReadEntity> Reports => Set<ReportReadEntity>();

    public DbSet<ProcessedDomainEventEntity> ProcessedDomainEvents => Set<ProcessedDomainEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserReadEntity>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Login).IsRequired().HasMaxLength(256);
            e.HasIndex(x => x.Login).IsUnique();
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
            e.Property(x => x.Role).IsRequired().HasConversion<int>();
            e.Property(x => x.IsActive).IsRequired();
            e.Property(x => x.Version).IsRequired();
        });

        modelBuilder.Entity<PatientReadEntity>(e =>
        {
            e.ToTable("patients");
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(512);
            e.Property(x => x.BirthDate).IsRequired();
            e.Property(x => x.Gender).HasMaxLength(32);
            e.Property(x => x.IsDeleted).IsRequired();
            e.Property(x => x.Version).IsRequired();
        });

        modelBuilder.Entity<TemplateReadEntity>(e =>
        {
            e.ToTable("templates");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(256);
            e.Property(x => x.StructureJson).IsRequired().HasColumnName("structure").HasColumnType("jsonb");
            e.Property(x => x.Version).IsRequired();
            e.Property(x => x.IsDeleted).IsRequired();
            e.HasMany(x => x.Keywords)
                .WithOne(x => x.Template!)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<KeywordReadEntity>(e =>
        {
            e.ToTable("keywords");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Phrase).IsRequired().HasMaxLength(256);
            e.Property(x => x.TargetField).IsRequired().HasMaxLength(256);
            e.HasIndex(x => new { x.TemplateId, x.Phrase }).IsUnique();
        });

        modelBuilder.Entity<ReportReadEntity>(e =>
        {
            e.ToTable("reports");
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).IsRequired().HasConversion<int>();
            e.Property(x => x.ContentJson).IsRequired().HasColumnName("content").HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).IsRequired();
            e.Property(x => x.UpdatedAt).IsRequired();
            e.Property(x => x.Version).IsRequired();
            e.Property(x => x.IsDeleted).IsRequired();
            e.HasIndex(x => x.PatientId);
            e.HasIndex(x => x.DoctorId);
            e.HasIndex(x => x.TemplateId);
            e.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Template)
                .WithMany()
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProcessedDomainEventEntity>(e =>
        {
            e.ToTable("processed_domain_events");
            e.HasKey(x => x.EventId);
            e.Property(x => x.ProcessedAt).IsRequired();
        });
    }
}
