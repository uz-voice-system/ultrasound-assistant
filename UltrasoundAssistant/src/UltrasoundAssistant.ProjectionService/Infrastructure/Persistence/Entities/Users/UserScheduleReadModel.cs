namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

public sealed class UserScheduleReadModel
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public bool IsDeleted { get; set; }

    public int Version { get; set; }

    public UserReadModel User { get; set; } = null!;
}
