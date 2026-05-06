using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;
using UltrasoundAssistant.ApiGateway.Infrastructure.Audit;
using UltrasoundAssistant.ApiGateway.Infrastructure.Persistence;
using UltrasoundAssistant.ApiGateway.Infrastructure.Persistence.Entities;
using UltrasoundAssistant.ApiGateway.Options;

namespace UltrasoundAssistant.ApiGateway.Middleware;

public sealed class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AuditOptions _options;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, IOptions<AuditOptions> options, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AuditDbContext auditDbContext)
    {
        if (!_options.Enabled || ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var startedAtUtc = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        string? rawRequestBody = null;
        string? sanitizedRequestBody = null;
        string? errorMessage = null;

        try
        {
            if (_options.CaptureRequestBody)
            {
                rawRequestBody = await ReadRequestBodyAsync(context.Request);

                sanitizedRequestBody = AuditSanitizer.SanitizeJson(
                    rawRequestBody,
                    _options.MaxRequestBodyLength);
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var finishedAtUtc = DateTime.UtcNow;
            var statusCode = context.Response.StatusCode;

            var entity = new AuditLogEntity
            {
                Id = Guid.NewGuid(),
                TraceId = context.TraceIdentifier,

                UserId = AuditRequestInfoExtractor.GetUserId(context),
                UserLogin = AuditRequestInfoExtractor.GetUserLogin(context),
                UserRole = AuditRequestInfoExtractor.GetUserRole(context),

                Method = context.Request.Method,
                Path = context.Request.Path.Value ?? string.Empty,
                QueryString = context.Request.QueryString.HasValue
                    ? context.Request.QueryString.Value
                    : null,
                Endpoint = context.GetEndpoint()?.DisplayName,

                Operation = AuditRequestInfoExtractor.GetOperation(context.Request),
                EntityType = AuditRequestInfoExtractor.GetEntityType(context.Request),
                EntityId = AuditRequestInfoExtractor.GetEntityId(context, rawRequestBody),

                StatusCode = statusCode,
                Succeeded = statusCode is >= 200 and < 400,
                ErrorMessage = errorMessage,

                StartedAtUtc = startedAtUtc,
                FinishedAtUtc = finishedAtUtc,
                DurationMs = stopwatch.ElapsedMilliseconds,

                ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),

                RequestBodyJson = sanitizedRequestBody
            };

            await TrySaveAuditAsync(auditDbContext, entity);
        }
    }

    private bool ShouldSkip(PathString path)
    {
        var value = path.Value ?? string.Empty;

        return _options.ExcludedPathPrefixes.Any(prefix =>
            value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength is null or 0)
            return null;

        if (!request.Body.CanSeek)
            request.EnableBuffering();

        request.Body.Position = 0;

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        request.Body.Position = 0;

        return body;
    }

    private async Task TrySaveAuditAsync(AuditDbContext dbContext, AuditLogEntity entity)
    {
        try
        {
            dbContext.AuditLogs.Add(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save audit log");
        }
    }
}
