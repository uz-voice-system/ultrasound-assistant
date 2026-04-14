using System.Text.Json;

namespace UltrasoundAssistant.ProjectionService.Application.Common;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
}