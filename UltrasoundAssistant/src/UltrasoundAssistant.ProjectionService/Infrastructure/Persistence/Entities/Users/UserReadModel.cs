using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

public sealed class UserReadModel
{
    public Guid Id { get; set; }

    public string Login { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public int Version { get; set; }

    public DoctorProfileReadModel? DoctorProfile { get; set; }

    public ICollection<UserScheduleReadModel> Schedules { get; set; } = new List<UserScheduleReadModel>();

    public ICollection<AppointmentReadModel> DoctorAppointments { get; set; } = new List<AppointmentReadModel>();

    public ICollection<AppointmentReadModel> CreatedAppointments { get; set; } = new List<AppointmentReadModel>();
}
