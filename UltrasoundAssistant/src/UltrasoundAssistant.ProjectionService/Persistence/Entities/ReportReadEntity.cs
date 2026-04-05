using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.ProjectionService.Persistence.Entities;

public sealed class ReportReadEntity
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public PatientReadEntity? Patient { get; set; }

    public Guid DoctorId { get; set; }

    public UserReadEntity? Doctor { get; set; }

    public Guid TemplateId { get; set; }

    public TemplateReadEntity? Template { get; set; }

    public ReportStatus Status { get; set; }

    public string ContentJson { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public int Version { get; set; }

    public bool IsDeleted { get; set; }
}
