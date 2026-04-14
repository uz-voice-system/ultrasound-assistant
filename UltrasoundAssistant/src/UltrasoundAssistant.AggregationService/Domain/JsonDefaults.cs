using System.Text.Json;

namespace UltrasoundAssistant.AggregationService.Domain;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
}
