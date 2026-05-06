using System.Text.Json;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;
using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.AppointmentEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class AppointmentAggregate
{
    public Guid Id { get; private set; }

    public bool Exists { get; private set; }

    public bool IsDeleted { get; private set; }

    public int Version { get; private set; }

    public Guid PatientId { get; private set; }

    public Guid DoctorId { get; private set; }

    public Guid TemplateId { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTime StartAtUtc { get; private set; }

    public DateTime EndAtUtc { get; private set; }

    public AppointmentStatus Status { get; private set; }

    public string? Comment { get; private set; }

    public AppointmentCreatedEvent Create(
        Guid appointmentId,
        Guid patientId,
        Guid doctorId,
        Guid templateId,
        Guid createdByUserId,
        DateTime startAtUtc,
        DateTime endAtUtc,
        string? comment)
    {
        if (Exists && !IsDeleted)
            throw new DomainException("Appointment already exists");

        var now = DateTime.UtcNow;

        return new AppointmentCreatedEvent
        {
            AppointmentId = appointmentId,
            PatientId = patientId,
            DoctorId = doctorId,
            TemplateId = templateId,
            CreatedByUserId = createdByUserId,
            StartAtUtc = startAtUtc,
            EndAtUtc = endAtUtc,
            Status = AppointmentStatus.Scheduled,
            Comment = NormalizeNullable(comment),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Version = Version + 1
        };
    }

    public AppointmentUpdatedEvent Update(
        Guid patientId,
        Guid doctorId,
        Guid templateId,
        DateTime startAtUtc,
        DateTime endAtUtc,
        AppointmentStatus status,
        string? comment)
    {
        if (!Exists || IsDeleted)
            throw new DomainException("Appointment not found");

        return new AppointmentUpdatedEvent
        {
            AppointmentId = Id,
            PatientId = patientId,
            DoctorId = doctorId,
            TemplateId = templateId,
            StartAtUtc = startAtUtc,
            EndAtUtc = endAtUtc,
            Status = status,
            Comment = NormalizeNullable(comment),
            UpdatedAtUtc = DateTime.UtcNow,
            Version = Version + 1
        };
    }

    public AppointmentDeletedEvent Delete()
    {
        if (!Exists || IsDeleted)
            throw new DomainException("Appointment not found or already deleted");

        return new AppointmentDeletedEvent
        {
            AppointmentId = Id,
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
            case nameof(AppointmentCreatedEvent):
                {
                    var e = JsonSerializer.Deserialize<AppointmentCreatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid AppointmentCreatedEvent payload");

                    Id = e.AppointmentId;
                    PatientId = e.PatientId;
                    DoctorId = e.DoctorId;
                    TemplateId = e.TemplateId;
                    CreatedByUserId = e.CreatedByUserId;
                    StartAtUtc = e.StartAtUtc;
                    EndAtUtc = e.EndAtUtc;
                    Status = e.Status;
                    Comment = e.Comment;
                    Exists = true;
                    IsDeleted = false;
                    Version = e.Version;
                    break;
                }

            case nameof(AppointmentUpdatedEvent):
                {
                    var e = JsonSerializer.Deserialize<AppointmentUpdatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid AppointmentUpdatedEvent payload");

                    PatientId = e.PatientId;
                    DoctorId = e.DoctorId;
                    TemplateId = e.TemplateId;
                    StartAtUtc = e.StartAtUtc;
                    EndAtUtc = e.EndAtUtc;
                    Status = e.Status;
                    Comment = e.Comment;
                    Exists = true;
                    IsDeleted = false;
                    Version = e.Version;
                    break;
                }

            case nameof(AppointmentDeletedEvent):
                {
                    var e = JsonSerializer.Deserialize<AppointmentDeletedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid AppointmentDeletedEvent payload");

                    IsDeleted = true;
                    Version = e.Version;
                    break;
                }
        }
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
