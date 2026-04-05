using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.PatientEvent;
using UltrasoundAssistant.Contracts.Events.ReportEvent;
using UltrasoundAssistant.Contracts.Events.TemplateEvent;
using UltrasoundAssistant.Contracts.Events.UserEvent;
using UltrasoundAssistant.ProjectionService.Persistence;
using UltrasoundAssistant.ProjectionService.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Services;

public sealed class DomainEventProcessor(ILogger<DomainEventProcessor> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task ProcessAsync(ReadDbContext db, string eventType, string json, CancellationToken cancellationToken)
    {
        Guid eventId;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("eventId", out var idEl))
            {
                logger.LogWarning("Event JSON missing eventId: {EventType}", eventType);
                return;
            }

            eventId = idEl.GetGuid();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invalid event JSON for {EventType}", eventType);
            return;
        }

        if (await db.ProcessedDomainEvents.AsNoTracking().AnyAsync(e => e.EventId == eventId, cancellationToken))
        {
            return;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            switch (eventType)
            {
                case nameof(PatientCreatedEvent):
                    await HandlePatientCreatedAsync(db, json, cancellationToken);
                    break;
                case nameof(PatientUpdatedEvent):
                    await HandlePatientUpdatedAsync(db, json, cancellationToken);
                    break;
                case nameof(PatientDeactivatedEvent):
                    await HandlePatientDeactivatedAsync(db, json, cancellationToken);
                    break;
                case nameof(TemplateCreatedEvent):
                    await HandleTemplateCreatedAsync(db, json, cancellationToken);
                    break;
                case nameof(TemplateUpdatedEvent):
                    await HandleTemplateUpdatedAsync(db, json, cancellationToken);
                    break;
                case nameof(ReportCreatedEvent):
                    await HandleReportCreatedAsync(db, json, cancellationToken);
                    break;
                case nameof(ReportFieldUpdatedEvent):
                    await HandleReportFieldUpdatedAsync(db, json, cancellationToken);
                    break;
                case nameof(ReportCompletedEvent):
                    await HandleReportCompletedAsync(db, json, cancellationToken);
                    break;
                case nameof(UserCreatedEvent):
                    await HandleUserCreatedAsync(db, json, cancellationToken);
                    break;
                case nameof(UserUpdatedEvent):
                    await HandleUserUpdatedAsync(db, json, cancellationToken);
                    break;
                case nameof(UserDeactivatedEvent):
                    await HandleUserDeactivatedAsync(db, json, cancellationToken);
                    break;
                case nameof(TemplateDeletedEvent):
                    await HandleTemplateDeletedAsync(db, json, cancellationToken);
                    break;
                case nameof(ReportDeletedEvent):
                    await HandleReportDeletedAsync(db, json, cancellationToken);
                    break;
                default:
                    logger.LogDebug("Ignoring unknown event type {EventType}", eventType);
                    break;
            }

            db.ProcessedDomainEvents.Add(new ProcessedDomainEventEntity
            {
                EventId = eventId,
                ProcessedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Failed to process domain event {EventType}", eventType);
            throw;
        }
    }

    private static async Task HandlePatientCreatedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<PatientCreatedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        if (await db.Patients.AnyAsync(p => p.Id == evt.Id, ct))
        {
            return;
        }

        db.Patients.Add(new PatientReadEntity
        {
            Id = evt.Id,
            FullName = evt.FullName,
            BirthDate = DateOnly.FromDateTime(evt.BirthDate.Date),
            Gender = evt.Gender,
            IsDeleted = false,
            Version = evt.Version
        });
    }

    private static async Task HandlePatientUpdatedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<PatientUpdatedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        var patient = await db.Patients.FirstOrDefaultAsync(p => p.Id == evt.PatientId, ct);
        if (patient is null)
        {
            return;
        }

        if (evt.Version <= patient.Version)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(evt.FullName))
        {
            patient.FullName = evt.FullName;
        }

        if (evt.BirthDate is not null)
        {
            patient.BirthDate = DateOnly.FromDateTime(evt.BirthDate.Value.Date);
        }

        patient.Gender = evt.Gender;
        patient.Version = evt.Version;
    }

    private static async Task HandlePatientDeactivatedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<PatientDeactivatedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        var patient = await db.Patients.FirstOrDefaultAsync(p => p.Id == evt.PatientId, ct);
        if (patient is null)
        {
            return;
        }

        if (evt.Version <= patient.Version)
        {
            return;
        }

        patient.IsDeleted = true;
        patient.Version = evt.Version;
    }

    private static async Task HandleTemplateCreatedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<TemplateCreatedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        if (await db.Templates.AnyAsync(t => t.Id == evt.TemplateId, ct))
        {
            return;
        }

        var structure = JsonSerializer.Serialize(new { keywords = evt.Keywords }, JsonOptions);
        var template = new TemplateReadEntity
        {
            Id = evt.TemplateId,
            Name = evt.Name,
            StructureJson = structure,
            Version = evt.Version,
            IsDeleted = false
        };
        db.Templates.Add(template);
        foreach (var kv in evt.Keywords)
        {
            db.Keywords.Add(new KeywordReadEntity
            {
                TemplateId = evt.TemplateId,
                Phrase = kv.Key,
                TargetField = kv.Value
            });
        }
    }

    private static async Task HandleTemplateUpdatedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<TemplateUpdatedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        var template = await db.Templates.Include(t => t.Keywords).FirstOrDefaultAsync(t => t.Id == evt.TemplateId, ct);
        if (template is null || template.IsDeleted)
        {
            return;
        }

        if (evt.Version <= template.Version)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(evt.Name))
        {
            template.Name = evt.Name;
        }

        if (evt.Keywords is not null)
        {
            template.StructureJson = JsonSerializer.Serialize(new { keywords = evt.Keywords }, JsonOptions);
            await db.Keywords.Where(k => k.TemplateId == evt.TemplateId).ExecuteDeleteAsync(ct);
            foreach (var kv in evt.Keywords)
            {
                db.Keywords.Add(new KeywordReadEntity
                {
                    TemplateId = evt.TemplateId,
                    Phrase = kv.Key,
                    TargetField = kv.Value
                });
            }
        }

        template.Version = evt.Version;
    }

    private static async Task EnsureDoctorUserAsync(ReadDbContext db, Guid doctorId, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Id == doctorId, ct))
        {
            return;
        }

        db.Users.Add(new UserReadEntity
        {
            Id = doctorId,
            Login = $"doctor-{doctorId:N}",
            PasswordHash = "PLACEHOLDER",
            Role = UserRole.Doctor,
            IsActive = true,
            Version = 0
        });
    }

    private static async Task HandleReportCreatedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<ReportCreatedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        if (await db.Reports.AnyAsync(r => r.Id == evt.Id, ct))
        {
            return;
        }

        await EnsureDoctorUserAsync(db, evt.DoctorId, ct);

        var now = DateTimeOffset.UtcNow;
        db.Reports.Add(new ReportReadEntity
        {
            Id = evt.Id,
            PatientId = evt.PatientId,
            DoctorId = evt.DoctorId,
            TemplateId = evt.TemplateId,
            Status = evt.Status,
            ContentJson = "{}",
            CreatedAt = now,
            UpdatedAt = now,
            Version = evt.Version,
            IsDeleted = false
        });
    }

    private static async Task HandleReportFieldUpdatedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<ReportFieldUpdatedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        var report = await db.Reports.FirstOrDefaultAsync(r => r.Id == evt.ReportId, ct);
        if (report is null || report.IsDeleted)
        {
            return;
        }

        if (evt.Version <= report.Version)
        {
            return;
        }

        var content = string.IsNullOrWhiteSpace(report.ContentJson) || report.ContentJson == "{}"
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : JsonSerializer.Deserialize<Dictionary<string, string>>(report.ContentJson, JsonOptions) ?? [];

        content[evt.FieldName] = evt.Value;
        report.ContentJson = JsonSerializer.Serialize(content, JsonOptions);
        report.UpdatedAt = DateTimeOffset.UtcNow;
        report.Version = evt.Version;
    }

    private static async Task HandleReportCompletedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<ReportCompletedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        var report = await db.Reports.FirstOrDefaultAsync(r => r.Id == evt.ReportId, ct);
        if (report is null || report.IsDeleted)
        {
            return;
        }

        if (evt.Version <= report.Version)
        {
            return;
        }

        report.Status = ReportStatus.Completed;
        report.UpdatedAt = DateTimeOffset.UtcNow;
        report.Version = evt.Version;
    }

    private static async Task HandleUserCreatedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<UserCreatedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        if (await db.Users.AnyAsync(u => u.Id == evt.Id, ct))
        {
            return;
        }

        db.Users.Add(new UserReadEntity
        {
            Id = evt.Id,
            Login = evt.Login,
            PasswordHash = evt.PasswordHash,
            Role = evt.Role,
            IsActive = true,
            Version = evt.Version
        });
    }

    private static async Task HandleUserUpdatedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<UserUpdatedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == evt.UserId, ct);
        if (user is null)
        {
            return;
        }

        if (evt.Version <= user.Version)
        {
            return;
        }

        user.Login = evt.Login;
        user.PasswordHash = evt.PasswordHash;
        user.Role = evt.Role;
        user.Version = evt.Version;
    }

    private static async Task HandleUserDeactivatedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<UserDeactivatedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == evt.UserId, ct);
        if (user is null)
        {
            return;
        }

        if (evt.Version <= user.Version)
        {
            return;
        }

        user.IsActive = false;
        user.Version = evt.Version;
    }

    private static async Task HandleTemplateDeletedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<TemplateDeletedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        var template = await db.Templates.FirstOrDefaultAsync(t => t.Id == evt.TemplateId, ct);
        if (template is null)
        {
            return;
        }

        if (evt.Version <= template.Version)
        {
            return;
        }

        template.IsDeleted = true;
        template.Version = evt.Version;
    }

    private static async Task HandleReportDeletedAsync(ReadDbContext db, string json, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<ReportDeletedEvent>(json, JsonOptions);
        if (evt is null)
        {
            return;
        }

        var report = await db.Reports.FirstOrDefaultAsync(r => r.Id == evt.ReportId, ct);
        if (report is null)
        {
            return;
        }

        if (evt.Version <= report.Version)
        {
            return;
        }

        report.IsDeleted = true;
        report.Version = evt.Version;
    }
}
