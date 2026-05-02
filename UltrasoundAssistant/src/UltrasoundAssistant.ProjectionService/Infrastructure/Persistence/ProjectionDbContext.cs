using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Templates;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

public sealed class ProjectionDbContext : DbContext
{
    public ProjectionDbContext(DbContextOptions<ProjectionDbContext> options)
        : base(options)
    {
    }

    public DbSet<PatientReadModel> Patients => Set<PatientReadModel>();
    public DbSet<TemplateReadModel> Templates => Set<TemplateReadModel>();
    public DbSet<TemplateBlockReadModel> TemplateBlocks => Set<TemplateBlockReadModel>();
    public DbSet<TemplateBlockPhraseReadModel> TemplateBlockPhrases => Set<TemplateBlockPhraseReadModel>();
    public DbSet<TemplateFieldReadModel> TemplateFields => Set<TemplateFieldReadModel>();
    public DbSet<TemplateFieldPhraseReadModel> TemplateFieldPhrases => Set<TemplateFieldPhraseReadModel>();
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

        template.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        template.Property(x => x.IsDeleted)
            .IsRequired();

        template.Property(x => x.Version)
            .IsRequired();

        template.HasIndex(x => x.Name);

        var block = modelBuilder.Entity<TemplateBlockReadModel>();

        block.ToTable("template_blocks");
        block.HasKey(x => x.Id);

        block.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        block.Property(x => x.Position)
            .IsRequired();

        block.Property(x => x.DefaultFieldName)
            .HasMaxLength(200);

        block.HasOne(x => x.Template)
            .WithMany(x => x.Blocks)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        block.HasIndex(x => new { x.TemplateId, x.Position });
        block.HasIndex(x => new { x.TemplateId, x.Name });

        var blockPhrase = modelBuilder.Entity<TemplateBlockPhraseReadModel>();

        blockPhrase.ToTable("template_block_phrases");
        blockPhrase.HasKey(x => x.Id);

        blockPhrase.Property(x => x.Phrase)
            .IsRequired()
            .HasMaxLength(300);

        blockPhrase.HasOne(x => x.Block)
            .WithMany(x => x.Phrases)
            .HasForeignKey(x => x.BlockId)
            .OnDelete(DeleteBehavior.Cascade);

        blockPhrase.HasIndex(x => new { x.BlockId, x.Phrase })
            .IsUnique();

        var field = modelBuilder.Entity<TemplateFieldReadModel>();

        field.ToTable("template_fields");
        field.HasKey(x => x.Id);

        field.Property(x => x.FieldName)
            .IsRequired()
            .HasMaxLength(200);

        field.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        field.Property(x => x.Position)
            .IsRequired();

        field.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        field.Property(x => x.NormMin)
            .HasColumnType("numeric(10,2)");

        field.Property(x => x.NormMax)
            .HasColumnType("numeric(10,2)");

        field.Property(x => x.NormUnit)
            .HasMaxLength(50);

        field.Property(x => x.NormNormalText)
            .HasMaxLength(500);

        field.HasOne(x => x.Block)
            .WithMany(x => x.Fields)
            .HasForeignKey(x => x.BlockId)
            .OnDelete(DeleteBehavior.Cascade);

        field.HasIndex(x => new { x.BlockId, x.Position });
        field.HasIndex(x => new { x.BlockId, x.FieldName }).IsUnique();

        var fieldPhrase = modelBuilder.Entity<TemplateFieldPhraseReadModel>();

        fieldPhrase.ToTable("template_field_phrases");
        fieldPhrase.HasKey(x => x.Id);

        fieldPhrase.Property(x => x.Phrase)
            .IsRequired()
            .HasMaxLength(300);

        fieldPhrase.HasOne(x => x.Field)
            .WithMany(x => x.Phrases)
            .HasForeignKey(x => x.FieldId)
            .OnDelete(DeleteBehavior.Cascade);

        fieldPhrase.HasIndex(x => new { x.FieldId, x.Phrase })
            .IsUnique();
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
