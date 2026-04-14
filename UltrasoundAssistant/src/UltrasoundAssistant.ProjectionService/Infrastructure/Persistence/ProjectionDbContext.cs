using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

public sealed class ProjectionDbContext : DbContext
{
    public ProjectionDbContext(DbContextOptions<ProjectionDbContext> options)
        : base(options)
    {
    }

    public DbSet<PatientReadModel> Patients => Set<PatientReadModel>();
    public DbSet<TemplateReadModel> Templates => Set<TemplateReadModel>();
    public DbSet<TemplateKeywordReadModel> TemplateKeywords => Set<TemplateKeywordReadModel>();
    public DbSet<ReportReadModel> Reports => Set<ReportReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurePatients(modelBuilder);
        ConfigureTemplates(modelBuilder);
        ConfigureReports(modelBuilder);
    }

    private static void ConfigurePatients(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<PatientReadModel>();

        entity.ToTable("patients");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.FullName).IsRequired().HasMaxLength(300);
        entity.Property(x => x.Gender).HasMaxLength(50);
        entity.Property(x => x.IsDeleted).IsRequired();
        entity.Property(x => x.Version).IsRequired();
    }

    private static void ConfigureTemplates(ModelBuilder modelBuilder)
    {
        var template = modelBuilder.Entity<TemplateReadModel>();

        template.ToTable("templates");
        template.HasKey(x => x.Id);

        template.Property(x => x.Name).IsRequired().HasMaxLength(200);
        template.Property(x => x.StructureJson).IsRequired();
        template.Property(x => x.IsDeleted).IsRequired();
        template.Property(x => x.Version).IsRequired();

        var keyword = modelBuilder.Entity<TemplateKeywordReadModel>();

        keyword.ToTable("keywords");
        keyword.HasKey(x => x.Id);

        keyword.Property(x => x.Phrase).IsRequired().HasMaxLength(200);
        keyword.Property(x => x.TargetField).IsRequired().HasMaxLength(200);

        keyword.HasOne(x => x.Template)
            .WithMany(x => x.Keywords)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        keyword.HasIndex(x => new { x.TemplateId, x.Phrase }).IsUnique();
    }

    private static void ConfigureReports(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ReportReadModel>();

        entity.ToTable("reports");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
        entity.Property(x => x.ContentJson).IsRequired();
        entity.Property(x => x.IsDeleted).IsRequired();
        entity.Property(x => x.CreatedAtUtc).IsRequired();
        entity.Property(x => x.UpdatedAtUtc).IsRequired();
        entity.Property(x => x.Version).IsRequired();

        entity.HasIndex(x => x.PatientId);
        entity.HasIndex(x => x.TemplateId);
    }
}