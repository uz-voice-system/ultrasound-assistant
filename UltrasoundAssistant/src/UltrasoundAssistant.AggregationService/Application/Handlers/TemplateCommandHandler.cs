using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Application.Common;
using UltrasoundAssistant.AggregationService.Application.Validation;
using UltrasoundAssistant.AggregationService.Domain;
using UltrasoundAssistant.Contracts.Commands.Templates;

namespace UltrasoundAssistant.AggregationService.Application.Handlers;

public sealed class TemplateCommandHandler : CommandHandlerBase
{
    private const string AggregateType = "template";
    private const string TemplateCreatedRoutingKey = "template.created";
    private const string TemplateUpdatedRoutingKey = "template.updated";
    private const string TemplateDeletedRoutingKey = "template.deleted";

    private readonly IEventStore _eventStore;

    public TemplateCommandHandler(
        IEventStore eventStore,
        IIntegrationEventPublisher publisher,
        IUnitOfWork unitOfWork,
        ILogger<TemplateCommandHandler> logger)
        : base(eventStore, publisher, unitOfWork, logger)
    {
        _eventStore = eventStore;
    }

    public async Task<CommandResult> CreateAsync(
        CreateTemplateCommand command,
        CancellationToken ct)
    {
        try
        {
            TemplateCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.TemplateId, ct);

            var @event = aggregate.Create(
                command.TemplateId,
                command.Name,
                command.Blocks);

            return await SaveAndPublishAsync(
                AggregateType,
                command.TemplateId,
                aggregate.Version,
                [EventFactory.Create(@event, TemplateCreatedRoutingKey)],
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

    public async Task<CommandResult> UpdateAsync(
        UpdateTemplateCommand command,
        CancellationToken ct)
    {
        try
        {
            TemplateCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.TemplateId, ct);

            if (!aggregate.Exists || aggregate.IsDeleted)
                return CommandResult.NotFound("Template not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict(
                    $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.Update(
                command.Name,
                command.Blocks);

            return await SaveAndPublishAsync(
                AggregateType,
                command.TemplateId,
                aggregate.Version,
                [EventFactory.Create(@event, TemplateUpdatedRoutingKey)],
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

    public async Task<CommandResult> DeleteAsync(
        DeleteTemplateCommand command,
        CancellationToken ct)
    {
        try
        {
            TemplateCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.TemplateId, ct);

            if (!aggregate.Exists || aggregate.IsDeleted)
                return CommandResult.NotFound("Template not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict(
                    $"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.Delete();

            return await SaveAndPublishAsync(
                AggregateType,
                command.TemplateId,
                aggregate.Version,
                [EventFactory.Create(@event, TemplateDeletedRoutingKey)],
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

    private async Task<TemplateAggregate> LoadAggregateAsync(
        Guid templateId,
        CancellationToken ct)
    {
        var history = await _eventStore.LoadAggregateEventsAsync(
            AggregateType,
            templateId,
            ct);

        var aggregate = new TemplateAggregate();
        aggregate.LoadFrom(history);

        return aggregate;
    }
}