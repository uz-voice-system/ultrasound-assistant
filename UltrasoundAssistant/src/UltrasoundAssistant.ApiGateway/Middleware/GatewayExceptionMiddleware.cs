using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace UltrasoundAssistant.ApiGateway.Middleware;

public sealed class GatewayExceptionMiddleware(RequestDelegate next, ILogger<GatewayExceptionMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Upstream HTTP error for {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
            context.Response.ContentType = "application/json";
            var detail = env.IsDevelopment() ? ex.Message : "Upstream service unreachable";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Bad gateway", detail }));
        }
        catch (TaskCanceledException ex) when (!context.RequestAborted.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Upstream timeout for {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Gateway timeout" }));
        }
    }
}
