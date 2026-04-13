namespace UltrasoundAssistant.AggregationService.Application.Common;

public sealed record CommandResult(int StatusCode, string Message)
{
    public static CommandResult Ok(string message) => new(200, message);
    public static CommandResult Accepted(string message) => new(202, message);
    public static CommandResult BadRequest(string message) => new(400, message);
    public static CommandResult NotFound(string message) => new(404, message);
    public static CommandResult Conflict(string message) => new(409, message);
    public static CommandResult ServerError(string message) => new(500, message);
}