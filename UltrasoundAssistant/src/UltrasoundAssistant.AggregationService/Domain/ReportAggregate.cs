using System.Text.Json;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;
using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.ReportEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class ReportAggregate
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid TemplateId { get; private set; }

    public bool Exists { get; private set; }
    public bool IsDeleted { get; private set; }
    public ReportStatus Status { get; private set; }
    public int Version { get; private set; }

    public Dictionary<string, ReportFieldState> Fields { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public ReportCreatedEvent Create(Guid reportId, Guid patientId, Guid doctorId, Guid templateId)
    {
        if (reportId == Guid.Empty)
            throw new DomainException("Report id is required");

        if (patientId == Guid.Empty)
            throw new DomainException("Patient id is required");

        if (doctorId == Guid.Empty)
            throw new DomainException("Doctor id is required");

        if (templateId == Guid.Empty)
            throw new DomainException("Template id is required");

        if (Exists)
            throw new DomainException("Report already exists");

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

    public ReportFieldUpdatedEvent UpdateField(string fieldName, string value, double confidence)
    {
        if (!Exists || IsDeleted)
            throw new DomainException("Report not found");

        if (Status == ReportStatus.Completed)
            throw new DomainException("Completed report cannot be changed");

        if (string.IsNullOrWhiteSpace(fieldName))
            throw new DomainException("Field name is required");

        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Field value is required");

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
            throw new DomainException("Report not found");

        if (Status == ReportStatus.Completed)
            throw new DomainException("Report already completed");

        return new ReportCompletedEvent
        {
            ReportId = Id,
            Version = Version + 1
        };
    }

    public ReportDeletedEvent DeleteDraft()
    {
        if (!Exists || IsDeleted)
            throw new DomainException("Report not found");

        return new ReportDeletedEvent
        {
            ReportId = Id,
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
            case nameof(ReportCreatedEvent):
                {
                    var e = JsonSerializer.Deserialize<ReportCreatedEvent>(record.Payload)!;
                    Id = e.Id;
                    PatientId = e.PatientId;
                    DoctorId = e.DoctorId;
                    TemplateId = e.TemplateId;
                    Status = e.Status;
                    Exists = true;
                    IsDeleted = false;
                    Version = e.Version;
                    break;
                }

            case nameof(ReportFieldUpdatedEvent):
                {
                    var e = JsonSerializer.Deserialize<ReportFieldUpdatedEvent>(record.Payload)!;
                    Fields[e.FieldName] = new ReportFieldState(e.Value, e.Confidence);
                    Version = e.Version;
                    break;
                }

            case nameof(ReportCompletedEvent):
                {
                    var e = JsonSerializer.Deserialize<ReportCompletedEvent>(record.Payload)!;
                    Status = ReportStatus.Completed;
                    Version = e.Version;
                    break;
                }

            case nameof(ReportDeletedEvent):
                {
                    var e = JsonSerializer.Deserialize<ReportDeletedEvent>(record.Payload)!;
                    IsDeleted = true;
                    Version = e.Version;
                    break;
                }
        }
    }
}

public sealed record ReportFieldState(string Value, double Confidence);