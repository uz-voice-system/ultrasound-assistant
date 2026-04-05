namespace UltrasoundAssistant.ProjectionService.Persistence.Entities;

public sealed class PatientReadEntity
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    public string? Gender { get; set; }

    public bool IsDeleted { get; set; }

    public int Version { get; set; }
}
