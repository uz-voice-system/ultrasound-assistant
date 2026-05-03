namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

public sealed class DoctorProfileReadModel
{
    public Guid UserId { get; set; }

    public string? Specialization { get; set; }

    public string? Cabinet { get; set; }

    public string? PhoneExtension { get; set; }

    public UserReadModel User { get; set; } = null!;
}
