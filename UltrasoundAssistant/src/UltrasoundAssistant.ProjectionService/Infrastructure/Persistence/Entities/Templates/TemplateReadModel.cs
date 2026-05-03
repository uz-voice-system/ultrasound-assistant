namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Templates;

public sealed class TemplateReadModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DefaultAppointmentDurationMinutes { get; set; } = 20;

    public bool IsDeleted { get; set; }

    public int Version { get; set; }

    public ICollection<TemplateBlockReadModel> Blocks { get; set; } = new List<TemplateBlockReadModel>();

    public ICollection<AppointmentReadModel> Appointments { get; set; } = new List<AppointmentReadModel>();
}
