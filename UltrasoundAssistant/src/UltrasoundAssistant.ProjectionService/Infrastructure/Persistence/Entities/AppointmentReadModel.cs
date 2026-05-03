using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Patients;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Templates;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

public sealed class AppointmentReadModel
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public Guid DoctorId { get; set; }

    public Guid TemplateId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public AppointmentStatus Status { get; set; }

    public string? Comment { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public int Version { get; set; }

    public PatientReadModel Patient { get; set; } = null!;

    public UserReadModel Doctor { get; set; } = null!;

    public UserReadModel CreatedByUser { get; set; } = null!;

    public TemplateReadModel Template { get; set; } = null!;

    public ReportReadModel? Report { get; set; }
}
