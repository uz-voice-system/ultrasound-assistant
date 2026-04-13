using System.Text.Json;
using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Application.Common;
using UltrasoundAssistant.AggregationService.Application.Validation;
using UltrasoundAssistant.AggregationService.Domain;
using UltrasoundAssistant.Contracts.Commands.Reports;
using UltrasoundAssistant.Contracts.Events.ReportEvent;

namespace UltrasoundAssistant.AggregationService.Application.Handlers;

public sealed class ReportCommandHandler : CommandHandlerBase
{
    private readonly IEventStore _eventStore;
    private readonly VoiceCommandMatcher _voiceCommandMatcher;

    public ReportCommandHandler(
        ICommandDeduplicationStore deduplicationStore,
        IEventStore eventStore,
        IIntegrationEventPublisher publisher,
        IUnitOfWork unitOfWork,
        VoiceCommandMatcher voiceCommandMatcher,
        ILogger<ReportCommandHandler> logger)
        : base(deduplicationStore, eventStore, publisher, unitOfWork, logger)
    {
        _eventStore = eventStore;
        _voiceCommandMatcher = voiceCommandMatcher;
    }

    public async Task<CommandResult> CreateAsync(CreateReportCommand command, CancellationToken ct)
    {
        try
        {
            ReportCommandValidator.Validate(command);

            var patientHistory = await _eventStore.LoadAggregateEventsAsync("patient", command.PatientId, ct);
            var patient = new PatientAggregate();
            patient.LoadFrom(patientHistory);

            if (!patient.Exists || !patient.IsActive)
                return CommandResult.NotFound("Patient is missing or inactive");

            var templateHistory = await _eventStore.LoadAggregateEventsAsync("template", command.TemplateId, ct);
            var template = new TemplateAggregate();
            template.LoadFrom(templateHistory);

            if (!template.Exists || template.IsDeleted)
                return CommandResult.NotFound("Template is missing or deleted");

            var reportHistory = await _eventStore.LoadAggregateEventsAsync("report", command.ReportId, ct);
            var report = new ReportAggregate();
            report.LoadFrom(reportHistory);

            var @event = report.Create(command.ReportId, command.PatientId, command.DoctorId, command.TemplateId);

            return await SaveAndPublishAsync(
                command.CommandId,
                "report",
                command.ReportId,
                report.Version,
                [EventFactory.Create(@event, "report.created")],
                ct);
        }
        catch (ArgumentException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
    }

    public async Task<CommandResult> UpdateFieldAsync(UpdateReportFieldCommand command, CancellationToken ct)
    {
        try
        {
            ReportCommandValidator.Validate(command);

            var history = await _eventStore.LoadAggregateEventsAsync("report", command.ReportId, ct);
            var report = new ReportAggregate();
            report.LoadFrom(history);

            if (!report.Exists || report.IsDeleted)
                return CommandResult.NotFound("Report not found");

            if (command.ExpectedVersion != report.Version)
                return CommandResult.Conflict($"Concurrency conflict. Expected {command.ExpectedVersion}, actual {report.Version}");

            var @event = report.UpdateField(command.FieldName, command.Value, command.Confidence);

            return await SaveAndPublishAsync(
                command.CommandId,
                "report",
                command.ReportId,
                report.Version,
                [EventFactory.Create(@event, "report.field.updated")],
                ct);
        }
        catch (ArgumentException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
    }

    public async Task<CommandResult> ProcessVoiceAsync(ProcessVoiceDataCommand command, CancellationToken ct)
    {
        try
        {
            ReportCommandValidator.Validate(command);

            var reportHistory = await _eventStore.LoadAggregateEventsAsync("report", command.ReportId, ct);
            var report = new ReportAggregate();
            report.LoadFrom(reportHistory);

            if (!report.Exists || report.IsDeleted)
                return CommandResult.NotFound("Report not found");

            if (command.ExpectedVersion != report.Version)
                return CommandResult.Conflict($"Concurrency conflict. Expected {command.ExpectedVersion}, actual {report.Version}");

            var templateHistory = await _eventStore.LoadAggregateEventsAsync("template", report.TemplateId, ct);
            var template = new TemplateAggregate();
            template.LoadFrom(templateHistory);

            if (!template.Exists || template.IsDeleted)
                return CommandResult.NotFound("Template not found");

            var match = ResolveVoiceMatch(command, template);

            if (!match.IsSuccess)
                return CommandResult.BadRequest(match.Error);

            var @event = report.UpdateField(match.FieldName, match.Value, command.Confidence);

            return await SaveAndPublishAsync(
                command.CommandId,
                "report",
                command.ReportId,
                report.Version,
                [EventFactory.Create(@event, "report.field.updated")],
                ct);
        }
        catch (ArgumentException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
    }

    public async Task<CommandResult> CompleteAsync(CompleteReportCommand command, CancellationToken ct)
    {
        try
        {
            ReportCommandValidator.Validate(command);

            var history = await _eventStore.LoadAggregateEventsAsync("report", command.ReportId, ct);
            var report = new ReportAggregate();
            report.LoadFrom(history);

            if (!report.Exists || report.IsDeleted)
                return CommandResult.NotFound("Report not found");

            if (command.ExpectedVersion != report.Version)
                return CommandResult.Conflict($"Concurrency conflict. Expected {command.ExpectedVersion}, actual {report.Version}");

            var @event = report.Complete();

            return await SaveAndPublishAsync(
                command.CommandId,
                "report",
                command.ReportId,
                report.Version,
                [EventFactory.Create(@event, "report.completed")],
                ct);
        }
        catch (ArgumentException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
    }

    public async Task<CommandResult> DeleteAsync(DeleteReportCommand command, CancellationToken ct)
    {
        try
        {
            ReportCommandValidator.Validate(command);

            var history = await _eventStore.LoadAggregateEventsAsync("report", command.ReportId, ct);
            var report = new ReportAggregate();
            report.LoadFrom(history);

            if (!report.Exists || report.IsDeleted)
                return CommandResult.NotFound("Report not found");

            if (command.ExpectedVersion != report.Version)
                return CommandResult.Conflict($"Concurrency conflict. Expected {command.ExpectedVersion}, actual {report.Version}");

            var @event = report.DeleteDraft();

            return await SaveAndPublishAsync(
                command.CommandId,
                "report",
                command.ReportId,
                report.Version,
                [EventFactory.Create(@event, "report.deleted")],
                ct);
        }
        catch (ArgumentException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
    }

    private VoiceMatchResult ResolveVoiceMatch(ProcessVoiceDataCommand command, TemplateAggregate template)
    {
        if (!string.IsNullOrWhiteSpace(command.DetectedKeyword) &&
            !string.IsNullOrWhiteSpace(command.DetectedValue))
        {
            if (template.Keywords.TryGetValue(command.DetectedKeyword.Trim(), out var targetField))
            {
                return VoiceMatchResult.Success(targetField, command.DetectedValue.Trim());
            }
        }

        return _voiceCommandMatcher.Match(command.RecognizedText, template.Keywords);
    }
}