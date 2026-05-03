using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Patients;

public sealed class PatientDocumentReadModel
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public PatientDocumentType DocumentType { get; set; }

    public string? Series { get; set; }

    public string Number { get; set; } = string.Empty;

    public string? IssuedBy { get; set; }

    public DateTime? IssueDate { get; set; }

    public string? DepartmentCode { get; set; }

    public string? Organization { get; set; }

    public PatientReadModel Patient { get; set; } = null!;
}