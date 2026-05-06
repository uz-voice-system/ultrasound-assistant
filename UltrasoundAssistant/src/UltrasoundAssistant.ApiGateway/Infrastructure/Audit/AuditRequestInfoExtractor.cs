using System.Security.Claims;
using System.Text.Json;

namespace UltrasoundAssistant.ApiGateway.Infrastructure.Audit;

public static class AuditRequestInfoExtractor
{
    private static readonly string[] EntityIdPropertyNames =
    [
        "id",
        "userId",
        "patientId",
        "templateId",
        "appointmentId",
        "reportId",
        "scheduleId"
    ];

    public static Guid? GetUserId(HttpContext context)
    {
        var value = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId)
            ? userId
            : null;
    }

    public static string? GetUserLogin(HttpContext context)
    {
        return context.User.Identity?.Name ??
               context.User.FindFirstValue(ClaimTypes.Name);
    }

    public static string? GetUserRole(HttpContext context)
    {
        return context.User.FindFirstValue(ClaimTypes.Role);
    }

    public static string GetOperation(HttpRequest request)
    {
        var path = request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        if (path.Contains("/auth/login"))
            return "Login";

        if (path.EndsWith("/search"))
            return "Search";

        if (path.Contains("/activate"))
            return "Activate";

        if (path.Contains("/deactivate"))
            return "Deactivate";

        return request.Method.ToUpperInvariant() switch
        {
            "GET" => "Read",
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "Patch",
            "DELETE" => "Delete",
            _ => request.Method
        };
    }

    public static string? GetEntityType(HttpRequest request)
    {
        var segments = request.Path.Value?
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments is null || segments.Length < 2)
            return null;

        if (!string.Equals(segments[0], "api", StringComparison.OrdinalIgnoreCase))
            return null;

        return segments[1].ToLowerInvariant() switch
        {
            "auth" => "auth",
            "users" => "user",
            "patients" => "patient",
            "templates" => "template",
            "schedules" => "schedule",
            "appointments" => "appointment",
            "reports" => "report",
            _ => segments[1]
        };
    }

    public static Guid? GetEntityId(HttpContext context, string? requestBody)
    {
        foreach (var routeValue in context.Request.RouteValues.Values)
        {
            if (routeValue is null)
                continue;

            if (Guid.TryParse(routeValue.ToString(), out var routeGuid))
                return routeGuid;
        }

        if (string.IsNullOrWhiteSpace(requestBody))
            return null;

        try
        {
            using var document = JsonDocument.Parse(requestBody);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            foreach (var propertyName in EntityIdPropertyNames)
            {
                if (!document.RootElement.TryGetProperty(propertyName, out var property))
                    continue;

                if (property.ValueKind != JsonValueKind.String)
                    continue;

                if (Guid.TryParse(property.GetString(), out var id))
                    return id;
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}
