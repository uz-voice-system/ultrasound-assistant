using System.Text.Json;
using UltrasoundAssistant.Contracts.Commands.Reports;

namespace UltrasoundAssistant.AggregationService.Application.Validation;

public static class ReportCommandValidator
{
    public static void Validate(CreateReportCommand command)
    {
        if (command.ReportId == Guid.Empty)
            throw new ArgumentException("ReportId is required");

        if (command.AppointmentId == Guid.Empty)
            throw new ArgumentException("AppointmentId is required");

        if (!Enum.IsDefined(command.Status))
            throw new ArgumentException($"Invalid report status: {command.Status}");

        ValidateContentJson(command.ContentJson);
    }

    public static void Validate(UpdateReportCommand command)
    {
        if (command.ReportId == Guid.Empty)
            throw new ArgumentException("ReportId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");

        if (!Enum.IsDefined(command.Status))
            throw new ArgumentException($"Invalid report status: {command.Status}");

        ValidateContentJson(command.ContentJson);
    }

    public static void Validate(DeleteReportCommand command)
    {
        if (command.ReportId == Guid.Empty)
            throw new ArgumentException("ReportId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }

    private static void ValidateContentJson(string contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
            throw new ArgumentException("ContentJson is required");

        try
        {
            JsonDocument.Parse(contentJson);
        }
        catch (JsonException)
        {
            throw new ArgumentException("ContentJson has invalid JSON format");
        }
    }
}
