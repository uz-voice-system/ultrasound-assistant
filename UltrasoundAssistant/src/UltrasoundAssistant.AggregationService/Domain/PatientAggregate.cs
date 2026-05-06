using System.Text.Json;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;
using UltrasoundAssistant.Contracts.Entity.Patients;
using UltrasoundAssistant.Contracts.Events.PatientEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class PatientAggregate
{
    public Guid Id { get; private set; }

    public bool Exists { get; private set; }

    public bool IsDeleted { get; private set; }

    public int Version { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public DateTime BirthDate { get; private set; }

    public string? Gender { get; private set; }

    public string? PhoneNumber { get; private set; }

    public string? Email { get; private set; }

    public List<PatientDocumentDto> Documents { get; private set; } = [];

    public PatientCreatedEvent Create(
        Guid patientId,
        string fullName,
        DateTime birthDate,
        string? gender,
        string? phoneNumber,
        string? email,
        IReadOnlyList<PatientDocumentDto> documents)
    {
        if (Exists)
            throw new DomainException("Patient already exists");

        return new PatientCreatedEvent
        {
            PatientId = patientId,
            FullName = fullName.Trim(),
            BirthDate = birthDate,
            Gender = NormalizeNullable(gender),
            PhoneNumber = NormalizeNullable(phoneNumber),
            Email = NormalizeNullable(email),
            Documents = CloneDocuments(documents),
            Version = Version + 1
        };
    }

    public PatientUpdatedEvent Update(
        string fullName,
        DateTime birthDate,
        string? gender,
        string? phoneNumber,
        string? email,
        IReadOnlyList<PatientDocumentDto> documents)
    {
        if (!Exists || IsDeleted)
            throw new DomainException("Patient not found");

        return new PatientUpdatedEvent
        {
            PatientId = Id,
            FullName = fullName.Trim(),
            BirthDate = birthDate,
            Gender = NormalizeNullable(gender),
            PhoneNumber = NormalizeNullable(phoneNumber),
            Email = NormalizeNullable(email),
            Documents = CloneDocuments(documents),
            Version = Version + 1
        };
    }

    public PatientDeletedEvent Delete()
    {
        if (!Exists || IsDeleted)
            throw new DomainException("Patient not found or already deleted");

        return new PatientDeletedEvent
        {
            PatientId = Id,
            Version = Version + 1
        };
    }

    public void LoadFrom(IEnumerable<EventRecord> history)
    {
        foreach (var item in history.OrderBy(x => x.Version))
            Apply(item);
    }

    private void Apply(EventRecord record)
    {
        switch (record.EventType)
        {
            case nameof(PatientCreatedEvent):
                {
                    var e = JsonSerializer.Deserialize<PatientCreatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid PatientCreatedEvent payload");

                    Id = e.PatientId;
                    FullName = e.FullName;
                    BirthDate = e.BirthDate;
                    Gender = e.Gender;
                    PhoneNumber = e.PhoneNumber;
                    Email = e.Email;
                    Documents = CloneDocuments(e.Documents);
                    Exists = true;
                    IsDeleted = false;
                    Version = e.Version;
                    break;
                }

            case nameof(PatientUpdatedEvent):
                {
                    var e = JsonSerializer.Deserialize<PatientUpdatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid PatientUpdatedEvent payload");

                    FullName = e.FullName;
                    BirthDate = e.BirthDate;
                    Gender = e.Gender;
                    PhoneNumber = e.PhoneNumber;
                    Email = e.Email;
                    Documents = CloneDocuments(e.Documents);
                    Exists = true;
                    IsDeleted = false;
                    Version = e.Version;
                    break;
                }

            case nameof(PatientDeletedEvent):
                {
                    var e = JsonSerializer.Deserialize<PatientDeletedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid PatientDeletedEvent payload");

                    IsDeleted = true;
                    Version = e.Version;
                    break;
                }
        }
    }

    private static List<PatientDocumentDto> CloneDocuments(
        IReadOnlyList<PatientDocumentDto> documents)
    {
        return documents
            .Select(document => new PatientDocumentDto
            {
                Id = document.Id,
                DocumentType = document.DocumentType,
                Series = NormalizeNullable(document.Series),
                Number = document.Number.Trim(),
                IssuedBy = NormalizeNullable(document.IssuedBy),
                IssueDate = document.IssueDate,
                DepartmentCode = NormalizeNullable(document.DepartmentCode),
                Organization = NormalizeNullable(document.Organization)
            })
            .ToList();
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
