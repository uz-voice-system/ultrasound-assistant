using UltrasoundAssistant.Contracts.Commands.Appointments;
using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.AggregationService.Application.Validation;

public static class AppointmentCommandValidator
{
    private const int MaxCommentLength = 1000;

    public static void Validate(CreateAppointmentCommand command)
    {
        if (command.AppointmentId == Guid.Empty)
            throw new ArgumentException("AppointmentId is required");

        ValidateCommon(
            command.PatientId,
            command.DoctorId,
            command.TemplateId,
            command.StartAtUtc,
            command.EndAtUtc,
            command.Comment);

        if (command.CreatedByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId is required");
    }

    public static void Validate(UpdateAppointmentCommand command)
    {
        if (command.AppointmentId == Guid.Empty)
            throw new ArgumentException("AppointmentId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");

        if (!Enum.IsDefined(command.Status))
            throw new ArgumentException($"Invalid appointment status: {command.Status}");

        ValidateCommon(
            command.PatientId,
            command.DoctorId,
            command.TemplateId,
            command.StartAtUtc,
            command.EndAtUtc,
            command.Comment);
    }

    public static void Validate(DeleteAppointmentCommand command)
    {
        if (command.AppointmentId == Guid.Empty)
            throw new ArgumentException("AppointmentId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }

    private static void ValidateCommon(
        Guid patientId,
        Guid doctorId,
        Guid templateId,
        DateTime startAtUtc,
        DateTime endAtUtc,
        string? comment)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("PatientId is required");

        if (doctorId == Guid.Empty)
            throw new ArgumentException("DoctorId is required");

        if (templateId == Guid.Empty)
            throw new ArgumentException("TemplateId is required");

        if (startAtUtc == default)
            throw new ArgumentException("StartAtUtc is required");

        if (endAtUtc == default)
            throw new ArgumentException("EndAtUtc is required");

        if (endAtUtc <= startAtUtc)
            throw new ArgumentException("EndAtUtc must be greater than StartAtUtc");

        if (!string.IsNullOrWhiteSpace(comment) && comment.Trim().Length > MaxCommentLength)
            throw new ArgumentException($"Comment length cannot exceed {MaxCommentLength}");
    }
}
