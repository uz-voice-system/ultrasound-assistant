using UltrasoundAssistant.Contracts.Commands.Reports;

namespace UltrasoundAssistant.AggregationService.Application.Validation;

public static class ReportCommandValidator
{
    public static void Validate(CreateReportCommand command)
    {
        if (command.CommandId == Guid.Empty)
            throw new ArgumentException("CommandId is required");

        if (command.ReportId == Guid.Empty)
            throw new ArgumentException("ReportId is required");

        if (command.PatientId == Guid.Empty)
            throw new ArgumentException("PatientId is required");

        if (command.DoctorId == Guid.Empty)
            throw new ArgumentException("DoctorId is required");

        if (command.TemplateId == Guid.Empty)
            throw new ArgumentException("TemplateId is required");
    }

    public static void Validate(UpdateReportFieldCommand command)
    {
        if (command.CommandId == Guid.Empty)
            throw new ArgumentException("CommandId is required");

        if (command.ReportId == Guid.Empty)
            throw new ArgumentException("ReportId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");

        if (string.IsNullOrWhiteSpace(command.FieldName))
            throw new ArgumentException("FieldName is required");

        if (string.IsNullOrWhiteSpace(command.Value))
            throw new ArgumentException("Value is required");
    }

    public static void Validate(ProcessVoiceDataCommand command)
    {
        if (command.CommandId == Guid.Empty)
            throw new ArgumentException("CommandId is required");

        if (command.ReportId == Guid.Empty)
            throw new ArgumentException("ReportId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");

        if (string.IsNullOrWhiteSpace(command.RecognizedText))
            throw new ArgumentException("RecognizedText is required");
    }

    public static void Validate(CompleteReportCommand command)
    {
        if (command.CommandId == Guid.Empty)
            throw new ArgumentException("CommandId is required");

        if (command.ReportId == Guid.Empty)
            throw new ArgumentException("ReportId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }

    public static void Validate(DeleteReportCommand command)
    {
        if (command.CommandId == Guid.Empty)
            throw new ArgumentException("CommandId is required");

        if (command.ReportId == Guid.Empty)
            throw new ArgumentException("ReportId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }
}