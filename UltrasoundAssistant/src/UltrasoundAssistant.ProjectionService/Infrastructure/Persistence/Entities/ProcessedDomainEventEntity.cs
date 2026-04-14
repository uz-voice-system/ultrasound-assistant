namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

/// <summary>
/// Идемпотентность обработки сообщений из брокера (по доменному EventId).
/// </summary>
public sealed class ProcessedDomainEventEntity
{
    public Guid EventId { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }
}
