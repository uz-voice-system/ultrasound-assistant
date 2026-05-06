using System.Text.Json;
using System.Text.Json.Nodes;

namespace UltrasoundAssistant.ApiGateway.Infrastructure.Audit;

public static class AuditSanitizer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> SensitivePropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "passwordHash",
        "token",
        "accessToken",
        "refreshToken",
        "authorization",
        "secret",
        "secretKey",
        "contentJson"
    };

    public static string? SanitizeJson(string? json, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        var trimmed = json.Trim();

        if (trimmed.Length > maxLength)
            return $"[body too large: {trimmed.Length} chars]";

        try
        {
            var node = JsonNode.Parse(trimmed);

            if (node is null)
                return null;

            Redact(node);

            var sanitized = node.ToJsonString(JsonOptions);

            return sanitized.Length > maxLength
                ? sanitized[..maxLength]
                : sanitized;
        }
        catch (JsonException)
        {
            return trimmed.Length > maxLength
                ? trimmed[..maxLength]
                : trimmed;
        }
    }

    private static void Redact(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                RedactObject(jsonObject);
                break;

            case JsonArray jsonArray:
                foreach (var item in jsonArray)
                    Redact(item);
                break;
        }
    }

    private static void RedactObject(JsonObject jsonObject)
    {
        var properties = jsonObject.ToList();

        foreach (var property in properties)
        {
            if (SensitivePropertyNames.Contains(property.Key))
            {
                jsonObject[property.Key] = "***";
                continue;
            }

            Redact(property.Value);
        }
    }
}
