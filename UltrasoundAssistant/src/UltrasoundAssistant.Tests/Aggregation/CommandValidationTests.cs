using UltrasoundAssistant.AggregationService.Application.Validation;
using UltrasoundAssistant.Contracts.Commands.Appointments;
using UltrasoundAssistant.Contracts.Commands.Reports;
using Xunit;

namespace UltrasoundAssistant.Tests.Aggregation;

public sealed class CommandValidationTests
{
    [Fact]
    public void Validate_CreateAppointment_WithoutPatient_ShouldThrow()
    {
        var command = CreateValidAppointmentCommand();
        command.PatientId = Guid.Empty;

        var exception = Assert.Throws<ArgumentException>(() =>
            AppointmentCommandValidator.Validate(command));

        Assert.Contains("Patient", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_CreateAppointment_WithoutDoctor_ShouldThrow()
    {
        var command = CreateValidAppointmentCommand();
        command.DoctorId = Guid.Empty;

        var exception = Assert.Throws<ArgumentException>(() =>
            AppointmentCommandValidator.Validate(command));

        Assert.Contains("Doctor", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_CreateAppointment_WithoutTemplate_ShouldThrow()
    {
        var command = CreateValidAppointmentCommand();
        command.TemplateId = Guid.Empty;

        var exception = Assert.Throws<ArgumentException>(() =>
            AppointmentCommandValidator.Validate(command));

        Assert.Contains("Template", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_CreateAppointment_WithEmptyStartDate_ShouldThrow()
    {
        var command = CreateValidAppointmentCommand();
        command.StartAtUtc = default;

        var exception = Assert.Throws<ArgumentException>(() =>
            AppointmentCommandValidator.Validate(command));

        Assert.Contains("Start", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_CreateAppointment_WithEndBeforeStart_ShouldThrow()
    {
        var command = CreateValidAppointmentCommand();
        command.StartAtUtc = new DateTime(2026, 5, 17, 10, 0, 0, DateTimeKind.Utc);
        command.EndAtUtc = new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Utc);

        var exception = Assert.Throws<ArgumentException>(() =>
            AppointmentCommandValidator.Validate(command));

        Assert.Contains("End", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_CreateReport_WithoutAppointment_ShouldThrow()
    {
        var command = CreateValidReportCommand();
        command.AppointmentId = Guid.Empty;

        var exception = Assert.Throws<ArgumentException>(() =>
            ReportCommandValidator.Validate(command));

        Assert.Contains("Appointment", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_CreateReport_ValidCommand_ShouldNotThrow()
    {
        var command = CreateValidReportCommand();

        var exception = Record.Exception(() =>
            ReportCommandValidator.Validate(command));

        Assert.Null(exception);
    }

    private static CreateAppointmentCommand CreateValidAppointmentCommand()
    {
        return new CreateAppointmentCommand
        {
            AppointmentId = Guid.Parse("71000000-0000-0000-0000-000000000001"),
            PatientId = Guid.Parse("72000000-0000-0000-0000-000000000001"),
            DoctorId = Guid.Parse("73000000-0000-0000-0000-000000000001"),
            TemplateId = Guid.Parse("74000000-0000-0000-0000-000000000001"),
            CreatedByUserId = Guid.Parse("70000000-0000-0000-0000-000000000001"),
            StartAtUtc = new DateTime(2026, 5, 17, 10, 0, 0, DateTimeKind.Utc),
            EndAtUtc = new DateTime(2026, 5, 17, 10, 30, 0, DateTimeKind.Utc),
            Comment = "Тестовая запись"
        };
    }

    private static CreateReportCommand CreateValidReportCommand()
    {
        return new CreateReportCommand
        {
            ReportId = Guid.Parse("75000000-0000-0000-0000-000000000001"),
            AppointmentId = Guid.Parse("71000000-0000-0000-0000-000000000001"),
            ContentJson = "{}"
        };
    }
}
