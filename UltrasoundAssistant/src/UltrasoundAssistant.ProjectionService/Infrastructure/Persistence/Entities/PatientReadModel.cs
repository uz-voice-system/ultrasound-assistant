namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

public sealed class PatientReadModel
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    public string? Gender { get; set; }

    public bool IsDeleted { get; set; }

    public int Version { get; set; }
}