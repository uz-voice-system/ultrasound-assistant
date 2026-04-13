using UltrasoundAssistant.Contracts.Commands.Patients;

namespace UltrasoundAssistant.AggregationService.Application.Validation;

public static class PatientCommandValidator
{
    public static void Validate(CreatePatientCommand command)
    {
        if (command.CommandId == Guid.Empty)
            throw new ArgumentException("CommandId is required");

        if (command.Id == Guid.Empty)
            throw new ArgumentException("Patient id is required");

        if (string.IsNullOrWhiteSpace(command.FullName))
            throw new ArgumentException("FullName is required");
    }

    public static void Validate(UpdatePatientCommand command)
    {
        if (command.CommandId == Guid.Empty)
            throw new ArgumentException("CommandId is required");

        if (command.PatientId == Guid.Empty)
            throw new ArgumentException("PatientId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }

    public static void Validate(DeactivatePatientCommand command)
    {
        if (command.CommandId == Guid.Empty)
            throw new ArgumentException("CommandId is required");

        if (command.PatientId == Guid.Empty)
            throw new ArgumentException("PatientId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }
}