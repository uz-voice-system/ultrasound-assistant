using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Patients;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Templates;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

public sealed class ProjectionDbContext : DbContext
{
    public ProjectionDbContext(DbContextOptions<ProjectionDbContext> options)
        : base(options)
    {
    }

    public DbSet<TemplateReadModel> Templates => Set<TemplateReadModel>();
    public DbSet<TemplateBlockReadModel> TemplateBlocks => Set<TemplateBlockReadModel>();
    public DbSet<TemplateBlockPhraseReadModel> TemplateBlockPhrases => Set<TemplateBlockPhraseReadModel>();
    public DbSet<TemplateFieldReadModel> TemplateFields => Set<TemplateFieldReadModel>();
    public DbSet<TemplateFieldPhraseReadModel> TemplateFieldPhrases => Set<TemplateFieldPhraseReadModel>();
    public DbSet<UserReadModel> Users => Set<UserReadModel>();
    public DbSet<DoctorProfileReadModel> DoctorProfiles => Set<DoctorProfileReadModel>();
    public DbSet<UserScheduleReadModel> UserSchedules => Set<UserScheduleReadModel>();
    public DbSet<PatientReadModel> Patients => Set<PatientReadModel>();
    public DbSet<PatientDocumentReadModel> PatientDocuments => Set<PatientDocumentReadModel>();
    public DbSet<AppointmentReadModel> Appointments => Set<AppointmentReadModel>();
    public DbSet<ReportReadModel> Reports => Set<ReportReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurePatients(modelBuilder);
        ConfigurePatientDocuments(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureDoctorProfiles(modelBuilder);
        ConfigureUserSchedules(modelBuilder);

        ConfigureTemplates(modelBuilder);
        ConfigureTemplateBlocks(modelBuilder);
        ConfigureTemplateBlockPhrases(modelBuilder);
        ConfigureTemplateFields(modelBuilder);
        ConfigureTemplateFieldPhrases(modelBuilder);

        ConfigureAppointments(modelBuilder);
        ConfigureReports(modelBuilder);
    }

    private static void ConfigurePatients(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<PatientReadModel>();

        entity.ToTable("patients");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.FullName).IsRequired().HasMaxLength(300);
        entity.Property(x => x.BirthDate).IsRequired();
        entity.Property(x => x.Gender).HasMaxLength(50);
        entity.Property(x => x.PhoneNumber).HasMaxLength(50);
        entity.Property(x => x.Email).HasMaxLength(200);
        entity.Property(x => x.IsDeleted).IsRequired();
        entity.Property(x => x.Version).IsRequired();

        entity.HasIndex(x => x.FullName);
        entity.HasIndex(x => x.BirthDate);
        entity.HasIndex(x => x.PhoneNumber);
    }

    private static void ConfigurePatientDocuments(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<PatientDocumentReadModel>();

        entity.ToTable("patient_documents");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.PatientId).IsRequired();

        entity.Property(x => x.DocumentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.Series).HasMaxLength(50);
        entity.Property(x => x.Number).IsRequired().HasMaxLength(100);
        entity.Property(x => x.IssuedBy).HasMaxLength(500);
        entity.Property(x => x.IssueDate);
        entity.Property(x => x.DepartmentCode).HasMaxLength(50);
        entity.Property(x => x.Organization).HasMaxLength(300);

        entity.HasOne(x => x.Patient)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.PatientId, x.DocumentType }).IsUnique();

        entity.HasIndex(x => new
        {
            x.DocumentType,
            x.Series,
            x.Number
        });

        entity.HasIndex(x => x.Number);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserReadModel>();

        entity.ToTable("users");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Login).IsRequired().HasMaxLength(100);
        entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
        entity.Property(x => x.FullName).IsRequired().HasMaxLength(300);

        entity.Property(x => x.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.IsActive).IsRequired();
        entity.Property(x => x.Version).IsRequired();

        entity.HasIndex(x => x.Login).IsUnique();
        entity.HasIndex(x => x.FullName);
        entity.HasIndex(x => x.Role);
        entity.HasIndex(x => x.IsActive);
    }

    private static void ConfigureDoctorProfiles(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<DoctorProfileReadModel>();

        entity.ToTable("doctor_profiles");
        entity.HasKey(x => x.UserId);

        entity.Property(x => x.Specialization).HasMaxLength(200);
        entity.Property(x => x.Cabinet).HasMaxLength(100);
        entity.Property(x => x.PhoneExtension).HasMaxLength(50);

        entity.HasOne(x => x.User)
            .WithOne(x => x.DoctorProfile)
            .HasForeignKey<DoctorProfileReadModel>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureUserSchedules(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserScheduleReadModel>();

        entity.ToTable("user_schedules");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.UserId).IsRequired();

        entity.Property(x => x.DayOfWeek)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.StartTime).IsRequired();
        entity.Property(x => x.EndTime).IsRequired();
        entity.Property(x => x.IsDeleted).IsRequired();
        entity.Property(x => x.Version).IsRequired();

        entity.HasOne(x => x.User)
            .WithMany(x => x.Schedules)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.UserId, x.DayOfWeek });
        entity.HasIndex(x => x.IsDeleted);
    }

    private static void ConfigureTemplates(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TemplateReadModel>();

        entity.ToTable("templates");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name).IsRequired().HasMaxLength(300);
        entity.Property(x => x.DefaultAppointmentDurationMinutes).IsRequired();
        entity.Property(x => x.IsDeleted).IsRequired();
        entity.Property(x => x.Version).IsRequired();

        entity.HasIndex(x => x.Name);
        entity.HasIndex(x => x.IsDeleted);
    }

    private static void ConfigureTemplateBlocks(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TemplateBlockReadModel>();

        entity.ToTable("template_blocks");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.TemplateId).IsRequired();
        entity.Property(x => x.Name).IsRequired().HasMaxLength(300);
        entity.Property(x => x.Position).IsRequired();
        entity.Property(x => x.DefaultFieldName).HasMaxLength(200);

        entity.HasOne(x => x.Template)
            .WithMany(x => x.Blocks)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.TemplateId, x.Position }).IsUnique();
        entity.HasIndex(x => new { x.TemplateId, x.Name });
    }

    private static void ConfigureTemplateBlockPhrases(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TemplateBlockPhraseReadModel>();

        entity.ToTable("template_block_phrases");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.BlockId).IsRequired();
        entity.Property(x => x.Phrase).IsRequired().HasMaxLength(300);

        entity.HasOne(x => x.Block)
            .WithMany(x => x.Phrases)
            .HasForeignKey(x => x.BlockId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.BlockId, x.Phrase }).IsUnique();
        entity.HasIndex(x => x.Phrase);
    }

    private static void ConfigureTemplateFields(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TemplateFieldReadModel>();

        entity.ToTable("template_fields");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.BlockId).IsRequired();
        entity.Property(x => x.FieldName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.DisplayName).IsRequired().HasMaxLength(300);
        entity.Property(x => x.Position).IsRequired();

        entity.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.NormMin).HasColumnType("numeric(18, 4)");
        entity.Property(x => x.NormMax).HasColumnType("numeric(18, 4)");
        entity.Property(x => x.NormUnit).HasMaxLength(50);
        entity.Property(x => x.NormNormalText).HasMaxLength(500);

        entity.HasOne(x => x.Block)
            .WithMany(x => x.Fields)
            .HasForeignKey(x => x.BlockId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.BlockId, x.FieldName }).IsUnique();
        entity.HasIndex(x => new { x.BlockId, x.Position }).IsUnique();
        entity.HasIndex(x => x.Role);
        entity.HasIndex(x => x.Type);
    }

    private static void ConfigureTemplateFieldPhrases(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TemplateFieldPhraseReadModel>();

        entity.ToTable("template_field_phrases");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.FieldId).IsRequired();
        entity.Property(x => x.Phrase).IsRequired().HasMaxLength(300);

        entity.HasOne(x => x.Field)
            .WithMany(x => x.Phrases)
            .HasForeignKey(x => x.FieldId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.FieldId, x.Phrase }).IsUnique();
        entity.HasIndex(x => x.Phrase);
    }

    private static void ConfigureAppointments(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AppointmentReadModel>();

        entity.ToTable("appointments");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.PatientId).IsRequired();
        entity.Property(x => x.DoctorId).IsRequired();
        entity.Property(x => x.TemplateId).IsRequired();
        entity.Property(x => x.CreatedByUserId).IsRequired();

        entity.Property(x => x.StartAtUtc).IsRequired();
        entity.Property(x => x.EndAtUtc).IsRequired();

        entity.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.Comment).HasMaxLength(1000);
        entity.Property(x => x.IsDeleted).IsRequired();
        entity.Property(x => x.CreatedAtUtc).IsRequired();
        entity.Property(x => x.UpdatedAtUtc).IsRequired();
        entity.Property(x => x.Version).IsRequired();

        entity.HasOne(x => x.Patient)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Doctor)
            .WithMany(x => x.DoctorAppointments)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedAppointments)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Template)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.DoctorId, x.StartAtUtc, x.EndAtUtc });
        entity.HasIndex(x => new { x.PatientId, x.StartAtUtc });
        entity.HasIndex(x => x.TemplateId);
        entity.HasIndex(x => x.CreatedByUserId);
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.IsDeleted);
    }

    private static void ConfigureReports(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ReportReadModel>();

        entity.ToTable("reports");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.AppointmentId).IsRequired();

        entity.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.ContentJson)
            .IsRequired()
            .HasColumnType("jsonb");

        entity.Property(x => x.IsDeleted).IsRequired();
        entity.Property(x => x.CreatedAtUtc).IsRequired();
        entity.Property(x => x.UpdatedAtUtc).IsRequired();
        entity.Property(x => x.Version).IsRequired();

        entity.HasOne(x => x.Appointment)
            .WithOne(x => x.Report)
            .HasForeignKey<ReportReadModel>(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.AppointmentId).IsUnique();
        entity.HasIndex(x => x.Status);
        entity.HasIndex(x => x.IsDeleted);
        entity.HasIndex(x => x.CreatedAtUtc);
    }
}
