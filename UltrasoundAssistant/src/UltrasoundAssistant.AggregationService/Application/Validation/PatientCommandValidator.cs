using UltrasoundAssistant.Contracts.Commands.Patients;
using UltrasoundAssistant.Contracts.Entity.Patients;

namespace UltrasoundAssistant.AggregationService.Application.Validation;

public static class PatientCommandValidator
{
    private const int MaxFullNameLength = 300;
    private const int MaxGenderLength = 50;
    private const int MaxPhoneLength = 50;
    private const int MaxEmailLength = 200;
    private const int MaxDocumentSeriesLength = 50;
    private const int MaxDocumentNumberLength = 100;
    private const int MaxIssuedByLength = 500;
    private const int MaxDepartmentCodeLength = 50;
    private const int MaxOrganizationLength = 300;

    public static void Validate(CreatePatientCommand command)
    {
        if (command.PatientId == Guid.Empty)
            throw new ArgumentException("PatientId is required");

        ValidatePatientData(
            command.FullName,
            command.BirthDate,
            command.Gender,
            command.PhoneNumber,
            command.Email,
            command.Documents);
    }

    public static void Validate(UpdatePatientCommand command)
    {
        if (command.PatientId == Guid.Empty)
            throw new ArgumentException("PatientId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");

        ValidatePatientData(
            command.FullName,
            command.BirthDate,
            command.Gender,
            command.PhoneNumber,
            command.Email,
            command.Documents);
    }

    public static void Validate(DeletePatientCommand command)
    {
        if (command.PatientId == Guid.Empty)
            throw new ArgumentException("PatientId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }

    private static void ValidatePatientData(
        string fullName,
        DateTime birthDate,
        string? gender,
        string? phoneNumber,
        string? email,
        IReadOnlyList<PatientDocumentDto> documents)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Patient full name is required");

        if (fullName.Trim().Length > MaxFullNameLength)
            throw new ArgumentException($"Patient full name length cannot exceed {MaxFullNameLength}");

        if (birthDate == default)
            throw new ArgumentException("Patient birth date is required");

        if (birthDate.Date > DateTime.UtcNow.Date)
            throw new ArgumentException("Patient birth date cannot be in the future");

        if (!string.IsNullOrWhiteSpace(gender) && gender.Trim().Length > MaxGenderLength)
            throw new ArgumentException($"Gender length cannot exceed {MaxGenderLength}");

        if (!string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Trim().Length > MaxPhoneLength)
            throw new ArgumentException($"Phone number length cannot exceed {MaxPhoneLength}");

        if (!string.IsNullOrWhiteSpace(email) && email.Trim().Length > MaxEmailLength)
            throw new ArgumentException($"Email length cannot exceed {MaxEmailLength}");

        ValidateDocuments(documents);
    }

    private static void ValidateDocuments(IReadOnlyList<PatientDocumentDto> documents)
    {
        if (documents is null)
            return;

        var documentIds = new HashSet<Guid>();
        var documentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var document in documents)
        {
            if (document.Id == Guid.Empty)
                throw new ArgumentException("Document id is required");

            if (!documentIds.Add(document.Id))
                throw new ArgumentException($"Duplicate document id: {document.Id}");

            if (!Enum.IsDefined(document.DocumentType))
                throw new ArgumentException($"Invalid document type: {document.DocumentType}");

            if (!documentTypes.Add(document.DocumentType.ToString()))
                throw new ArgumentException($"Duplicate document type: {document.DocumentType}");

            if (!string.IsNullOrWhiteSpace(document.Series) && document.Series.Trim().Length > MaxDocumentSeriesLength)
                throw new ArgumentException($"Document series length cannot exceed {MaxDocumentSeriesLength}");

            if (string.IsNullOrWhiteSpace(document.Number))
                throw new ArgumentException($"Document number is required: {document.DocumentType}");

            if (document.Number.Trim().Length > MaxDocumentNumberLength)
                throw new ArgumentException($"Document number length cannot exceed {MaxDocumentNumberLength}");

            if (!string.IsNullOrWhiteSpace(document.IssuedBy) && document.IssuedBy.Trim().Length > MaxIssuedByLength)
                throw new ArgumentException($"IssuedBy length cannot exceed {MaxIssuedByLength}");

            if (!string.IsNullOrWhiteSpace(document.DepartmentCode) && document.DepartmentCode.Trim().Length > MaxDepartmentCodeLength)
                throw new ArgumentException($"DepartmentCode length cannot exceed {MaxDepartmentCodeLength}");

            if (!string.IsNullOrWhiteSpace(document.Organization) && document.Organization.Trim().Length > MaxOrganizationLength)
                throw new ArgumentException($"Organization length cannot exceed {MaxOrganizationLength}");
        }
    }
}
