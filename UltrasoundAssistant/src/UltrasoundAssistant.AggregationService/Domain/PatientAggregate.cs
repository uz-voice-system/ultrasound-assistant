using UltrasoundAssistant.AggregationService.Infrastructure;
using UltrasoundAssistant.Contracts.Events.PatientEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class PatientAggregate
{
    public Guid Id { get; private set; }
    public bool Exists { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Version { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public DateTime BirthDate { get; private set; }
    public string? Gender { get; private set; }

    public void LoadFrom(IEnumerable<EventRecordEnvelope> events)
    {
        foreach (var item in events)
        {
            Apply(item.EventType, item.Payload, item.Version);
        }
    }

    public PatientCreatedEvent Create(Guid id, string fullName, DateTime birthDateUtc, string? gender)
    {
        if (Exists)
        {
            throw new DomainException("Patient already exists");
        }

        if (id == Guid.Empty || string.IsNullOrWhiteSpace(fullName) || birthDateUtc == default)
        {
            throw new DomainException("Invalid patient data");
        }

        if (birthDateUtc.Date > DateTime.UtcNow.Date)
        {
            throw new DomainException("Birth date cannot be in the future");
        }

        return new PatientCreatedEvent
        {
            Id = id,
            FullName = fullName.Trim(),
            BirthDate = birthDateUtc,
            Gender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim(),
            Version = Version + 1
        };
    }

    public PatientUpdatedEvent Update(string? fullName, DateTime? birthDate, string? gender)
    {
        if (!Exists || !IsActive)
        {
            throw new DomainException("Patient not found or inactive");
        }

        if (fullName is null && birthDate is null && gender is null)
        {
            throw new DomainException("At least one field must be provided");
        }

        var newName = fullName is null ? FullName : fullName.Trim();
        var newBirth = birthDate ?? BirthDate;
        var newGender = gender is null ? Gender : (string.IsNullOrWhiteSpace(gender) ? null : gender.Trim());

        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new DomainException("Full name is required");
        }

        if (newBirth == default || newBirth.Date > DateTime.UtcNow.Date)
        {
            throw new DomainException("Invalid birth date");
        }

        return new PatientUpdatedEvent
        {
            PatientId = Id,
            FullName = newName,
            BirthDate = newBirth,
            Gender = newGender,
            Version = Version + 1
        };
    }

    public PatientDeactivatedEvent Deactivate(Guid patientId, string? reason)
    {
        if (!Exists || patientId != Id)
        {
            throw new DomainException("Patient not found");
        }

        if (!IsActive)
        {
            throw new DomainException("Patient already deactivated");
        }

        return new PatientDeactivatedEvent
        {
            PatientId = patientId,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            IsDeleted = true,
            Version = Version + 1
        };
    }

    public void Apply(string eventType, string payload, int version)
    {
        switch (eventType)
        {
            case nameof(PatientCreatedEvent):
                var created = System.Text.Json.JsonSerializer.Deserialize<PatientCreatedEvent>(payload)
                    ?? throw new DomainException("Failed to deserialize PatientCreatedEvent");
                Id = created.Id;
                Exists = true;
                IsActive = true;
                FullName = created.FullName;
                BirthDate = created.BirthDate;
                Gender = created.Gender;
                Version = version;
                break;
            case nameof(PatientUpdatedEvent):
                var updated = System.Text.Json.JsonSerializer.Deserialize<PatientUpdatedEvent>(payload)
                    ?? throw new DomainException("Failed to deserialize PatientUpdatedEvent");
                Id = updated.PatientId;
                Exists = true;
                FullName = updated.FullName ?? string.Empty;
                BirthDate = updated.BirthDate ?? BirthDate;
                Gender = updated.Gender;
                Version = version;
                break;
            case nameof(PatientDeactivatedEvent):
                IsActive = false;
                Version = version;
                break;
        }
    }
}
