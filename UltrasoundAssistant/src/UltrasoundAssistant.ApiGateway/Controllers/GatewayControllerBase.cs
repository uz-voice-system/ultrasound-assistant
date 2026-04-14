using Microsoft.AspNetCore.Mvc;

namespace UltrasoundAssistant.ApiGateway.Controllers;

[ApiController]
public abstract class GatewayControllerBase : ControllerBase
{
    protected IActionResult ProxyJson(int statusCode, string content)
    {
        return new ContentResult
        {
            StatusCode = statusCode,
            Content = content,
            ContentType = "application/json"
        };
    }
}