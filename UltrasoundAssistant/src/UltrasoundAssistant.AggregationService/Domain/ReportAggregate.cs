using System.Text.Json;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;
using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.ReportEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class ReportAggregate
{
    public Guid Id { get; private set; }

    public bool Exists { get; private set; }

    public bool IsDeleted { get; private set; }

    public int Version { get; private set; }

    public Guid AppointmentId { get; private set; }

    public ReportStatus Status { get; private set; }

    public string ContentJson { get; private set; } = "{}";

    public ReportCreatedEvent Create(
        Guid reportId,
        Guid appointmentId,
        ReportStatus status,
        string contentJson)
    {
        if (Exists && !IsDeleted)
            throw new DomainException("Report already exists");

        var now = DateTime.UtcNow;

        return new ReportCreatedEvent
        {
            ReportId = reportId,
            AppointmentId = appointmentId,
            Status = status,
            ContentJson = contentJson,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Version = Version + 1
        };
    }

    public ReportUpdatedEvent Update(
        ReportStatus status,
        string contentJson)
    {
        if (!Exists || IsDeleted)
            throw new DomainException("Report not found");

        return new ReportUpdatedEvent
        {
            ReportId = Id,
            Status = status,
            ContentJson = contentJson,
            UpdatedAtUtc = DateTime.UtcNow,
            Version = Version + 1
        };
    }

    public ReportDeletedEvent Delete()
    {
        if (!Exists || IsDeleted)
            throw new DomainException("Report not found or already deleted");

        return new ReportDeletedEvent
        {
            ReportId = Id,
            UpdatedAtUtc = DateTime.UtcNow,
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
            case nameof(ReportCreatedEvent):
                {
                    var e = JsonSerializer.Deserialize<ReportCreatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid ReportCreatedEvent payload");

                    Id = e.ReportId;
                    AppointmentId = e.AppointmentId;
                    Status = e.Status;
                    ContentJson = e.ContentJson;
                    Exists = true;
                    IsDeleted = false;
                    Version = e.Version;
                    break;
                }

            case nameof(ReportUpdatedEvent):
                {
                    var e = JsonSerializer.Deserialize<ReportUpdatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid ReportUpdatedEvent payload");

                    Status = e.Status;
                    ContentJson = e.ContentJson;
                    Exists = true;
                    IsDeleted = false;
                    Version = e.Version;
                    break;
                }

            case nameof(ReportDeletedEvent):
                {
                    var e = JsonSerializer.Deserialize<ReportDeletedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid ReportDeletedEvent payload");

                    IsDeleted = true;
                    Version = e.Version;
                    break;
                }
        }
    }
}
