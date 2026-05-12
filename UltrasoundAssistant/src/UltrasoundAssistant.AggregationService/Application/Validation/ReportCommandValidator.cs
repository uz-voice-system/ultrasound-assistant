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

    private const int MaxImageBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png"
    };

    public static void Validate(UploadReportImageCommand command)
    {
        if (command.ReportId == Guid.Empty)
            throw new ArgumentException("ReportId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");

        if (string.IsNullOrWhiteSpace(command.FileName))
            throw new ArgumentException("FileName is required");

        if (command.FileName.Trim().Length > 255)
            throw new ArgumentException("FileName length cannot exceed 255");

        if (string.IsNullOrWhiteSpace(command.ContentType))
            throw new ArgumentException("ContentType is required");

        if (!AllowedImageContentTypes.Contains(command.ContentType.Trim()))
            throw new ArgumentException("Only JPEG and PNG images are supported");

        if (string.IsNullOrWhiteSpace(command.ImageBase64))
            throw new ArgumentException("ImageBase64 is required");

        byte[] bytes;

        try
        {
            bytes = Convert.FromBase64String(command.ImageBase64);
        }
        catch
        {
            throw new ArgumentException("ImageBase64 is invalid");
        }

        if (bytes.Length == 0)
            throw new ArgumentException("Image is empty");

        if (bytes.Length > MaxImageBytes)
            throw new ArgumentException($"Image size cannot exceed {MaxImageBytes} bytes");
    }

    public static void Validate(DeleteReportImageCommand command)
    {
        if (command.ReportId == Guid.Empty)
            throw new ArgumentException("ReportId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }
}
