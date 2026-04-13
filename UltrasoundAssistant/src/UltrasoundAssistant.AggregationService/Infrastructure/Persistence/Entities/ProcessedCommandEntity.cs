namespace UltrasoundAssistant.AggregationService.Infrastructure.Persistence.Entities;

public sealed class ProcessedCommandEntity
{
    public Guid CommandId { get; set; }

    public DateTime ProcessedAtUtc { get; set; }
}