namespace UltrasoundAssistant.AuditService.Persistence.Entities;

public sealed class AuditLogEntity
{
    public long Id { get; set; }

    public Guid? UserId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public Guid? EntityId { get; set; }

    public string DetailsJson { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; set; }
}
