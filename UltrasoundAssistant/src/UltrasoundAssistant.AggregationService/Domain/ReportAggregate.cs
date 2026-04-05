using UltrasoundAssistant.AggregationService.Infrastructure;
using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.ReportEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class ReportAggregate
{
    public Guid Id { get; private set; }
    public bool Exists { get; private set; }
    public bool IsDeleted { get; private set; }
    public int Version { get; private set; }
    public ReportStatus Status { get; private set; } = ReportStatus.Draft;
    public Dictionary<string, string> Fields { get; } = new(StringComparer.OrdinalIgnoreCase);

    public void LoadFrom(IEnumerable<EventRecordEnvelope> events)
    {
        foreach (var item in events)
        {
            Apply(item.EventType, item.Payload, item.Version);
        }
    }

    public ReportCreatedEvent Create(Guid reportId, Guid patientId, Guid doctorId, Guid templateId)
    {
        if (Exists)
        {
            throw new DomainException("Report already exists");
        }

        if (reportId == Guid.Empty || patientId == Guid.Empty || doctorId == Guid.Empty || templateId == Guid.Empty)
        {
            throw new DomainException("Invalid report data");
        }

        return new ReportCreatedEvent
        {
            Id = reportId,
            PatientId = patientId,
            DoctorId = doctorId,
            TemplateId = templateId,
            Status = ReportStatus.Draft,
            Version = Version + 1
        };
    }

    public ReportDeletedEvent DeleteDraft()
    {
        if (!Exists || IsDeleted)
        {
            throw new DomainException("Report not found");
        }

        if (Status != ReportStatus.Draft)
        {
            throw new DomainException("Only draft reports can be deleted");
        }

        return new ReportDeletedEvent
        {
            ReportId = Id,
            Version = Version + 1
        };
    }

    public ReportFieldUpdatedEvent UpdateField(string fieldName, string value, double confidence)
    {
        if (!Exists || IsDeleted)
        {
            throw new DomainException("Report not found");
        }

        if (Status == ReportStatus.Completed)
        {
            throw new DomainException("Cannot update completed report");
        }

        if (string.IsNullOrWhiteSpace(fieldName) || string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Field name and value are required");
        }

        if (confidence is < 0 or > 1)
        {
            throw new DomainException("Confidence should be in range [0, 1]");
        }

        return new ReportFieldUpdatedEvent
        {
            ReportId = Id,
            FieldName = fieldName.Trim(),
            Value = value.Trim(),
            Confidence = confidence,
            Version = Version + 1
        };
    }

    public ReportCompletedEvent Complete()
    {
        if (!Exists || IsDeleted)
        {
            throw new DomainException("Report not found");
        }

        if (Status == ReportStatus.Completed)
        {
            throw new DomainException("Report already completed");
        }

        if (Fields.Count == 0)
        {
            throw new DomainException("Cannot complete report without filled fields");
        }

        return new ReportCompletedEvent
        {
            ReportId = Id,
            Version = Version + 1
        };
    }

    private void Apply(string eventType, string payload, int version)
    {
        switch (eventType)
        {
            case nameof(ReportCreatedEvent):
                var created = System.Text.Json.JsonSerializer.Deserialize<ReportCreatedEvent>(payload)
                    ?? throw new DomainException("Failed to deserialize ReportCreatedEvent");
                Id = created.Id;
                Exists = true;
                IsDeleted = false;
                Status = created.Status;
                Version = version;
                break;
            case nameof(ReportFieldUpdatedEvent):
                var updated = System.Text.Json.JsonSerializer.Deserialize<ReportFieldUpdatedEvent>(payload)
                    ?? throw new DomainException("Failed to deserialize ReportFieldUpdatedEvent");
                Fields[updated.FieldName] = updated.Value;
                Version = version;
                break;
            case nameof(ReportCompletedEvent):
                Status = ReportStatus.Completed;
                Version = version;
                break;
            case nameof(ReportDeletedEvent):
                IsDeleted = true;
                Version = version;
                break;
        }
    }
}
