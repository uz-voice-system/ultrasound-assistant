using System.Text.Json;
using UltrasoundAssistant.ProjectionService.Application.Common;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers;

internal static class EventPayloadReader
{
    public static TEvent Read<TEvent>(string payload, string eventName)
    {
        return JsonSerializer.Deserialize<TEvent>(payload, JsonDefaults.Web)
            ?? throw new InvalidOperationException($"Invalid {eventName} payload");
    }
}
