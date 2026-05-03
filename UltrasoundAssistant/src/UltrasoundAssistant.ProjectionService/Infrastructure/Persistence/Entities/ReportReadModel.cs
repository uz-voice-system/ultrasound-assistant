using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

public sealed class ReportReadModel
{
    public Guid Id { get; set; }

    public Guid AppointmentId { get; set; }

    public ReportStatus Status { get; set; }

    public string ContentJson { get; set; } = "{}";

    public bool IsDeleted { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public int Version { get; set; }

    public AppointmentReadModel Appointment { get; set; } = null!;
}
