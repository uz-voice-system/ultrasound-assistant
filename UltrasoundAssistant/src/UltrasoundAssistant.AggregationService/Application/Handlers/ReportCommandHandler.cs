using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Application.Common;
using UltrasoundAssistant.AggregationService.Application.Validation;
using UltrasoundAssistant.AggregationService.Domain;
using UltrasoundAssistant.Contracts.Commands.Reports;

namespace UltrasoundAssistant.AggregationService.Application.Handlers;

public sealed class ReportCommandHandler : CommandHandlerBase
{
    private const string AggregateType = "report";
    private const string ReportCreatedRoutingKey = "report.created";
    private const string ReportUpdatedRoutingKey = "report.updated";
    private const string ReportDeletedRoutingKey = "report.deleted";
    private const string ReportImageUploadedRoutingKey = "report.image.uploaded";
    private const string ReportImageDeletedRoutingKey = "report.image.deleted";

    private readonly IEventStore _eventStore;

    public ReportCommandHandler(
        IEventStore eventStore,
        IIntegrationEventPublisher publisher,
        IUnitOfWork unitOfWork,
        ILogger<ReportCommandHandler> logger)
        : base(eventStore, publisher, unitOfWork, logger)
    {
        _eventStore = eventStore;
    }

    public async Task<CommandResult> CreateAsync(CreateReportCommand command, CancellationToken ct)
    {
        try
        {
            ReportCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.ReportId, ct);

            var @event = aggregate.Create(
                command.ReportId,
                command.AppointmentId,
                command.Status,
                command.ContentJson);

            return await SaveAndPublishAsync(
                AggregateType,
                command.ReportId,
                aggregate.Version,
                [EventFactory.Create(@event, ReportCreatedRoutingKey)],
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

    public async Task<CommandResult> UpdateAsync(UpdateReportCommand command, CancellationToken ct)
    {
        try
        {
            ReportCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.ReportId, ct);

            if (!aggregate.Exists || aggregate.IsDeleted)
                return CommandResult.NotFound("Report not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict(
                    $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.Update(
                command.Status,
                command.ContentJson);

            return await SaveAndPublishAsync(
                AggregateType,
                command.ReportId,
                aggregate.Version,
                [EventFactory.Create(@event, ReportUpdatedRoutingKey)],
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

            var aggregate = await LoadAggregateAsync(command.ReportId, ct);

            if (!aggregate.Exists || aggregate.IsDeleted)
                return CommandResult.NotFound("Report not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict(
                    $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.Delete();

            return await SaveAndPublishAsync(
                AggregateType,
                command.ReportId,
                aggregate.Version,
                [EventFactory.Create(@event, ReportDeletedRoutingKey)],
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

    public async Task<CommandResult> UploadImageAsync(UploadReportImageCommand command, CancellationToken ct)
    {
        try
        {
            ReportCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.ReportId, ct);

            if (!aggregate.Exists || aggregate.IsDeleted)
                return CommandResult.NotFound("Report not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict(
                    $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.UploadImage(
                command.FileName,
                command.ContentType,
                command.ImageBase64);

            return await SaveAndPublishAsync(
                AggregateType,
                command.ReportId,
                aggregate.Version,
                [EventFactory.Create(@event, ReportImageUploadedRoutingKey)],
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

    public async Task<CommandResult> DeleteImageAsync(DeleteReportImageCommand command, CancellationToken ct)
    {
        try
        {
            ReportCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.ReportId, ct);

            if (!aggregate.Exists || aggregate.IsDeleted)
                return CommandResult.NotFound("Report not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict(
                    $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.DeleteImage();

            return await SaveAndPublishAsync(
                AggregateType,
                command.ReportId,
                aggregate.Version,
                [EventFactory.Create(@event, ReportImageDeletedRoutingKey)],
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

    private async Task<ReportAggregate> LoadAggregateAsync(Guid reportId, CancellationToken ct)
    {
        var history = await _eventStore.LoadAggregateEventsAsync(
            AggregateType,
            reportId,
            ct);

        var aggregate = new ReportAggregate();
        aggregate.LoadFrom(history);

        return aggregate;
    }
}
