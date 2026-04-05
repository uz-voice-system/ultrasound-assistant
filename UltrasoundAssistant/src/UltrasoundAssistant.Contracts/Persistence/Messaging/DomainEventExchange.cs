namespace UltrasoundAssistant.Contracts.Persistence.Messaging;

/// <summary>
/// Имя exchange для доменных событий (Aggregation → брокер → Projection/Audit).
/// </summary>
public static class DomainEventExchange
{
    public const string Name = "ultrasound.domain.events";
}
