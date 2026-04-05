namespace UltrasoundAssistant.AggregationService.Persistence.Entities;

public sealed class ProcessedCommandEntity
{
    public Guid CommandId { get; set; }

    public string CommandType { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }
}
