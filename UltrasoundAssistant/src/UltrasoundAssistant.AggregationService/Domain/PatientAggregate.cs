using System.Text.Json;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;
using UltrasoundAssistant.Contracts.Events.PatientEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class PatientAggregate
{
    public Guid Id { get; private set; }
    public bool Exists { get; private set; }
    public bool IsActive { get; private set; }
    public int Version { get; private set; }

    public string FullName { get; private set; } = string.Empty;
    public DateTime BirthDate { get; private set; }
    public string? Gender { get; private set; }

    public PatientCreatedEvent Create(Guid id, string fullName, DateTime birthDate, string? gender)
    {
        if (id == Guid.Empty)
            throw new DomainException("Patient id is required");

        if (Exists)
            throw new DomainException("Patient already exists");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Patient full name is required");

        return new PatientCreatedEvent
        {
            Id = id,
            FullName = fullName.Trim(),
            BirthDate = birthDate,
            Gender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim(),
            Version = Version + 1
        };
    }

    public PatientUpdatedEvent Update(string? fullName, DateTime? birthDate, string? gender)
    {
        if (!Exists || !IsActive)
            throw new DomainException("Patient not found or inactive");

        var nextFullName = string.IsNullOrWhiteSpace(fullName) ? FullName : fullName.Trim();
        var nextBirthDate = birthDate ?? BirthDate;
        var nextGender = gender is null ? Gender : gender.Trim();

        if (string.IsNullOrWhiteSpace(nextFullName))
            throw new DomainException("Patient full name is required");

        return new PatientUpdatedEvent
        {
            PatientId = Id,
            FullName = nextFullName,
            BirthDate = nextBirthDate,
            Gender = nextGender,
            Version = Version + 1
        };
    }

    public PatientDeactivatedEvent Deactivate(Guid patientId, string? reason)
    {
        if (!Exists || !IsActive)
            throw new DomainException("Patient not found or already inactive");

        if (patientId != Id)
            throw new DomainException("Patient id mismatch");

        return new PatientDeactivatedEvent
        {
            PatientId = patientId,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            IsDeleted = true,
            Version = Version + 1
        };
    }

    public void LoadFrom(IEnumerable<EventRecord> history)
    {
        foreach (var item in history.OrderBy(x => x.Version))
        {
            Apply(item);
        }
    }

    private void Apply(EventRecord record)
    {
        switch (record.EventType)
        {
            case nameof(PatientCreatedEvent):
                {
                    var e = JsonSerializer.Deserialize<PatientCreatedEvent>(record.Payload)!;
                    Id = e.Id;
                    FullName = e.FullName;
                    BirthDate = e.BirthDate;
                    Gender = e.Gender;
                    Exists = true;
                    IsActive = true;
                    Version = e.Version;
                    break;
                }

            case nameof(PatientUpdatedEvent):
                {
                    var e = JsonSerializer.Deserialize<PatientUpdatedEvent>(record.Payload)!;
                    FullName = e.FullName ?? FullName;
                    BirthDate = e.BirthDate ?? BirthDate;
                    Gender = e.Gender ?? Gender;
                    Version = e.Version;
                    break;
                }

            case nameof(PatientDeactivatedEvent):
                {
                    var e = JsonSerializer.Deserialize<PatientDeactivatedEvent>(record.Payload)!;
                    IsActive = !e.IsDeleted;
                    Version = e.Version;
                    break;
                }
        }
    }
}