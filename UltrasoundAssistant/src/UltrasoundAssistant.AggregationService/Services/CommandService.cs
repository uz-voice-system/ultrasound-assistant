using System.Security.Cryptography;
using System.Text.Json;
using UltrasoundAssistant.AggregationService.Domain;
using UltrasoundAssistant.AggregationService.Infrastructure;
using UltrasoundAssistant.Contracts.Commands.Patients;
using UltrasoundAssistant.Contracts.Commands.Reports;
using UltrasoundAssistant.Contracts.Commands.Templates;
using UltrasoundAssistant.Contracts.Commands.Users;
using UltrasoundAssistant.Contracts.Events.CommandEvent;
using UltrasoundAssistant.Contracts.Persistence.EventStore.Contracts;

namespace UltrasoundAssistant.AggregationService.Services;

public sealed class CommandService(IEventStore eventStore, ILogger<CommandService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<(int StatusCode, string Message)> CreatePatientAsync(CreatePatientCommand command, Guid commandId, CancellationToken ct)
    {
        if (commandId == Guid.Empty)
        {
            return (400, "X-Command-Id header is required");
        }

        var history = await eventStore.LoadAggregateEventsAsync("patient", command.Id, ct);
        var aggregate = new PatientAggregate();
        aggregate.LoadFrom(history);

        try
        {
            var @event = aggregate.Create(command.Id, command.FullName, command.BirthDate.ToUniversalTime(), command.Gender);
            return await SaveAsync(commandId, nameof(CreatePatientCommand), "patient", command.Id, aggregate.Version, [ToEventRecord(@event, "patient.created")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> UpdatePatientAsync(UpdatePatientCommand command, CancellationToken ct)
    {
        var history = await eventStore.LoadAggregateEventsAsync("patient", command.PatientId, ct);
        var aggregate = new PatientAggregate();
        aggregate.LoadFrom(history);

        if (command.ExpectedVersion != aggregate.Version)
        {
            return (409, $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
        }

        try
        {
            var @event = aggregate.Update(command.FullName, command.BirthDate, command.Gender);
            return await SaveAsync(command.CommandId, nameof(UpdatePatientCommand), "patient", command.PatientId, aggregate.Version, [ToEventRecord(@event, "patient.updated")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> DeactivatePatientAsync(DeactivatePatientCommand command, CancellationToken ct)
    {
        var history = await eventStore.LoadAggregateEventsAsync("patient", command.PatientId, ct);
        var aggregate = new PatientAggregate();
        aggregate.LoadFrom(history);

        if (command.ExpectedVersion != aggregate.Version)
        {
            return (409, $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
        }

        try
        {
            var @event = aggregate.Deactivate(command.PatientId, command.Reason);
            return await SaveAsync(command.CommandId, nameof(DeactivatePatientCommand), "patient", command.PatientId, aggregate.Version, [ToEventRecord(@event, "patient.deactivated")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> CreateUserAsync(CreateUserCommand command, CancellationToken ct)
    {
        if (command.CommandId == Guid.Empty)
        {
            return (400, "CommandId is required");
        }

        var history = await eventStore.LoadAggregateEventsAsync("user", command.UserId, ct);
        var aggregate = new UserAggregate();
        aggregate.LoadFrom(history);

        try
        {
            var hash = HashPassword(command.Password);
            var @event = aggregate.Create(command.UserId, command.Login, hash, command.Role);
            return await SaveAsync(command.CommandId, nameof(CreateUserCommand), "user", command.UserId, aggregate.Version, [ToEventRecord(@event, "user.created")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> UpdateUserAsync(UpdateUserCommand command, CancellationToken ct)
    {
        var history = await eventStore.LoadAggregateEventsAsync("user", command.UserId, ct);
        var aggregate = new UserAggregate();
        aggregate.LoadFrom(history);

        if (command.ExpectedVersion != aggregate.Version)
        {
            return (409, $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
        }

        try
        {
            string? newHash = command.Password is null ? null : HashPassword(command.Password);
            var @event = aggregate.Update(command.Login, newHash, command.Role);
            return await SaveAsync(command.CommandId, nameof(UpdateUserCommand), "user", command.UserId, aggregate.Version, [ToEventRecord(@event, "user.updated")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> DeactivateUserAsync(DeactivateUserCommand command, CancellationToken ct)
    {
        var history = await eventStore.LoadAggregateEventsAsync("user", command.UserId, ct);
        var aggregate = new UserAggregate();
        aggregate.LoadFrom(history);

        if (command.ExpectedVersion != aggregate.Version)
        {
            return (409, $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
        }

        try
        {
            var @event = aggregate.Deactivate(command.UserId);
            return await SaveAsync(command.CommandId, nameof(DeactivateUserCommand), "user", command.UserId, aggregate.Version, [ToEventRecord(@event, "user.deactivated")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> CreateTemplateAsync(CreateTemplateCommand command, CancellationToken ct)
    {
        var history = await eventStore.LoadAggregateEventsAsync("template", command.TemplateId, ct);
        var aggregate = new TemplateAggregate();
        aggregate.LoadFrom(history);

        try
        {
            var @event = aggregate.Create(command.TemplateId, command.Name, command.Keywords);
            return await SaveAsync(command.CommandId, nameof(CreateTemplateCommand), "template", command.TemplateId, aggregate.Version, [ToEventRecord(@event, "template.created")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> UpdateTemplateAsync(UpdateTemplateCommand command, CancellationToken ct)
    {
        var history = await eventStore.LoadAggregateEventsAsync("template", command.TemplateId, ct);
        var aggregate = new TemplateAggregate();
        aggregate.LoadFrom(history);

        if (command.ExpectedVersion != aggregate.Version)
        {
            return (409, $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
        }

        try
        {
            var @event = aggregate.Update(command.Name, command.Keywords);
            return await SaveAsync(command.CommandId, nameof(UpdateTemplateCommand), "template", command.TemplateId, aggregate.Version, [ToEventRecord(@event, "template.updated")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> DeleteTemplateAsync(DeleteTemplateCommand command, CancellationToken ct)
    {
        var history = await eventStore.LoadAggregateEventsAsync("template", command.TemplateId, ct);
        var aggregate = new TemplateAggregate();
        aggregate.LoadFrom(history);

        if (command.ExpectedVersion != aggregate.Version)
        {
            return (409, $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
        }

        try
        {
            var @event = aggregate.Delete();
            return await SaveAsync(command.CommandId, nameof(DeleteTemplateCommand), "template", command.TemplateId, aggregate.Version, [ToEventRecord(@event, "template.deleted")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> CreateReportAsync(CreateReportCommand command, CancellationToken ct)
    {
        var patientHistory = await eventStore.LoadAggregateEventsAsync("patient", command.PatientId, ct);
        var patient = new PatientAggregate();
        patient.LoadFrom(patientHistory);
        if (!patient.Exists || !patient.IsActive)
        {
            return (404, "Patient is missing or inactive");
        }

        var templateHistory = await eventStore.LoadAggregateEventsAsync("template", command.TemplateId, ct);
        var template = new TemplateAggregate();
        template.LoadFrom(templateHistory);
        if (!template.Exists || template.IsDeleted)
        {
            return (404, "Template is missing or deleted");
        }

        var reportHistory = await eventStore.LoadAggregateEventsAsync("report", command.ReportId, ct);
        var report = new ReportAggregate();
        report.LoadFrom(reportHistory);

        try
        {
            var @event = report.Create(command.ReportId, command.PatientId, command.DoctorId, command.TemplateId);
            return await SaveAsync(command.CommandId, nameof(CreateReportCommand), "report", command.ReportId, report.Version, [ToEventRecord(@event, "report.created")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> UpdateReportFieldAsync(UpdateReportFieldCommand command, CancellationToken ct)
    {
        var history = await eventStore.LoadAggregateEventsAsync("report", command.ReportId, ct);
        var report = new ReportAggregate();
        report.LoadFrom(history);

        if (!report.Exists || report.IsDeleted)
        {
            return (404, "Report not found");
        }

        if (command.ExpectedVersion != report.Version)
        {
            return (409, $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {report.Version}");
        }

        try
        {
            var @event = report.UpdateField(command.FieldName, command.Value, command.Confidence);
            return await SaveAsync(command.CommandId, nameof(UpdateReportFieldCommand), "report", command.ReportId, report.Version, [ToEventRecord(@event, "report.field.updated")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> DeleteReportAsync(DeleteReportCommand command, CancellationToken ct)
    {
        var history = await eventStore.LoadAggregateEventsAsync("report", command.ReportId, ct);
        var aggregate = new ReportAggregate();
        aggregate.LoadFrom(history);

        if (command.ExpectedVersion != aggregate.Version)
        {
            return (409, $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
        }

        try
        {
            var @event = aggregate.DeleteDraft();
            return await SaveAsync(command.CommandId, nameof(DeleteReportCommand), "report", command.ReportId, aggregate.Version, [ToEventRecord(@event, "report.deleted")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> ProcessVoiceAsync(ProcessVoiceDataCommand command, Guid commandId, int expectedVersion, CancellationToken ct)
    {
        if (commandId == Guid.Empty)
        {
            return (400, "X-Command-Id header is required");
        }

        var reportHistory = await eventStore.LoadAggregateEventsAsync("report", command.ReportId, ct);
        var report = new ReportAggregate();
        report.LoadFrom(reportHistory);
        if (expectedVersion != report.Version)
        {
            return (409, $"Concurrency conflict. Expected {expectedVersion}, actual {report.Version}");
        }

        if (!report.Exists || report.IsDeleted)
        {
            return (404, "Report not found");
        }

        var reportCreatedPayload = reportHistory.FirstOrDefault(x => x.EventType == "ReportCreatedEvent")?.Payload;
        if (string.IsNullOrWhiteSpace(reportCreatedPayload))
        {
            return (404, "Report metadata not found");
        }

        var reportCreated = JsonSerializer.Deserialize<UltrasoundAssistant.Contracts.Events.ReportEvent.ReportCreatedEvent>(reportCreatedPayload, JsonOptions);
        if (reportCreated is null)
        {
            return (500, "Failed to read report aggregate");
        }

        var templateHistory = await eventStore.LoadAggregateEventsAsync("template", reportCreated.TemplateId, ct);
        var template = new TemplateAggregate();
        template.LoadFrom(templateHistory);
        if (!template.Exists || template.IsDeleted)
        {
            return (404, "Template not found");
        }

        var (field, value) = ExtractVoiceField(command, template);
        if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(value))
        {
            return (400, "Voice command does not match template keywords");
        }

        try
        {
            var @event = report.UpdateField(field, value, command.Confidence);
            return await SaveAsync(commandId, nameof(ProcessVoiceDataCommand), "report", command.ReportId, report.Version, [ToEventRecord(@event, "report.field.updated")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    public async Task<(int StatusCode, string Message)> CompleteReportAsync(CompleteReportCommand command, CancellationToken ct)
    {
        var history = await eventStore.LoadAggregateEventsAsync("report", command.ReportId, ct);
        var report = new ReportAggregate();
        report.LoadFrom(history);

        if (!report.Exists || report.IsDeleted)
        {
            return (404, "Report not found");
        }

        if (command.ExpectedVersion != report.Version)
        {
            return (409, $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {report.Version}");
        }

        try
        {
            var @event = report.Complete();
            return await SaveAsync(command.CommandId, nameof(CompleteReportCommand), "report", command.ReportId, report.Version, [ToEventRecord(@event, "report.completed")], ct);
        }
        catch (DomainException ex)
        {
            return (400, ex.Message);
        }
    }

    private async Task<(int StatusCode, string Message)> SaveAsync(
        Guid commandId,
        string commandType,
        string aggregateType,
        Guid aggregateId,
        int expectedVersion,
        IReadOnlyList<EventRecord> events,
        CancellationToken cancellationToken)
    {
        if (commandId == Guid.Empty)
        {
            return (400, "CommandId is required");
        }

        var request = new EventStoreAppendRequest
        {
            CommandId = commandId,
            CommandType = commandType,
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            ExpectedVersion = expectedVersion,
            Events = events.Select(ToAppendItem).ToList()
        };

        var result = await eventStore.AppendWithOutboxAsync(request, cancellationToken);

        return result switch
        {
            AppendResult.Success => (202, $"Accepted: {events[0].EventType}"),
            AppendResult.DuplicateCommand => (200, "Command already processed"),
            AppendResult.ConcurrencyConflict => (409, "Concurrency conflict while writing events"),
            _ => (500, "Unexpected write result")
        };
    }

    private static EventRecord ToEventRecord<TEvent>(TEvent @event, string routingKey)
    {
        return new EventRecord
        {
            EventId = Guid.NewGuid(),
            EventType = typeof(TEvent).Name,
            Payload = JsonSerializer.Serialize(@event, JsonOptions),
            Version = (int)(typeof(TEvent).GetProperty("Version")?.GetValue(@event) ?? 0),
            RoutingKey = routingKey,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static EventStoreAppendItem ToAppendItem(EventRecord record)
    {
        return new EventStoreAppendItem
        {
            EventId = record.EventId,
            EventType = record.EventType,
            RoutingKey = record.RoutingKey,
            Payload = record.Payload,
            Version = record.Version,
            CreatedAtUtc = record.CreatedAtUtc
        };
    }

    private static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new DomainException("Password cannot be empty");
        }

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private (string Field, string Value) ExtractVoiceField(ProcessVoiceDataCommand command, TemplateAggregate template)
    {
        if (!string.IsNullOrWhiteSpace(command.DetectedKeyword) && !string.IsNullOrWhiteSpace(command.DetectedValue))
        {
            return (command.DetectedKeyword.Trim(), command.DetectedValue.Trim());
        }

        var text = command.RecognizedText.Trim();
        foreach (var keyword in template.Keywords)
        {
            var idx = text.IndexOf(keyword.Key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                continue;
            }

            var valueStart = idx + keyword.Key.Length;
            var value = text[valueStart..].Trim().Trim(':', '-', ' ');
            if (!string.IsNullOrWhiteSpace(value))
            {
                return (keyword.Value, value);
            }
        }

        var delimiter = text.IndexOf(':');
        if (delimiter > 0 && delimiter < text.Length - 1)
        {
            return (text[..delimiter].Trim(), text[(delimiter + 1)..].Trim());
        }

        logger.LogWarning("Voice parsing failed for report {ReportId}. Payload: {Text}", command.ReportId, command.RecognizedText);
        return (string.Empty, string.Empty);
    }
}
