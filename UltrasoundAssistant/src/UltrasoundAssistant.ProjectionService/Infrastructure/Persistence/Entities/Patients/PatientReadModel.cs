namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Patients;

public sealed class PatientReadModel
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    public string? Gender { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public bool IsDeleted { get; set; }

    public int Version { get; set; }

    public ICollection<PatientDocumentReadModel> Documents { get; set; } = new List<PatientDocumentReadModel>();

    public ICollection<AppointmentReadModel> Appointments { get; set; } = new List<AppointmentReadModel>();
}
