using System.Text.Json;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;

namespace UltrasoundAssistant.AggregationService.Application.Common;

public static class EventFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static EventRecord Create<TEvent>(TEvent @event, string routingKey)
    {
        var eventId = (Guid)(typeof(TEvent).GetProperty("EventId")?.GetValue(@event)
                     ?? throw new InvalidOperationException("EventId property is missing"));

        var createdAt = (DateTime)(typeof(TEvent).GetProperty("CreatedAt")?.GetValue(@event)
                        ?? DateTime.UtcNow);

        var version = (int)(typeof(TEvent).GetProperty("Version")?.GetValue(@event)
                     ?? 0);

        return new EventRecord
        {
            EventId = eventId,
            EventType = typeof(TEvent).Name,
            Payload = JsonSerializer.Serialize(@event, JsonOptions),
            Version = version,
            RoutingKey = routingKey,
            CreatedAtUtc = createdAt
        };
    }
}