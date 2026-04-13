using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Application.Common;
using UltrasoundAssistant.AggregationService.Application.Validation;
using UltrasoundAssistant.AggregationService.Domain;
using UltrasoundAssistant.Contracts.Commands.Templates;

namespace UltrasoundAssistant.AggregationService.Application.Handlers;

public sealed class TemplateCommandHandler : CommandHandlerBase
{
    private readonly IEventStore _eventStore;

    public TemplateCommandHandler(
        ICommandDeduplicationStore deduplicationStore,
        IEventStore eventStore,
        IIntegrationEventPublisher publisher,
        IUnitOfWork unitOfWork,
        ILogger<TemplateCommandHandler> logger)
        : base(deduplicationStore, eventStore, publisher, unitOfWork, logger)
    {
        _eventStore = eventStore;
    }

    public async Task<CommandResult> CreateAsync(CreateTemplateCommand command, CancellationToken ct)
    {
        try
        {
            TemplateCommandValidator.Validate(command);

            var history = await _eventStore.LoadAggregateEventsAsync("template", command.TemplateId, ct);
            var aggregate = new TemplateAggregate();
            aggregate.LoadFrom(history);

            var @event = aggregate.Create(command.TemplateId, command.Name, command.Keywords);

            return await SaveAndPublishAsync(
                command.CommandId,
                "template",
                command.TemplateId,
                aggregate.Version,
                [EventFactory.Create(@event, "template.created")],
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

    public async Task<CommandResult> UpdateAsync(UpdateTemplateCommand command, CancellationToken ct)
    {
        try
        {
            TemplateCommandValidator.Validate(command);

            var history = await _eventStore.LoadAggregateEventsAsync("template", command.TemplateId, ct);
            var aggregate = new TemplateAggregate();
            aggregate.LoadFrom(history);

            if (!aggregate.Exists || aggregate.IsDeleted)
                return CommandResult.NotFound("Template not found");

            if (command.ExpectedVersion != aggregate.Version)
                return CommandResult.Conflict($"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");

            var @event = aggregate.Update(command.Name, command.Keywords);

            return await SaveAndPublishAsync(
                command.CommandId,
                "template",
                command.TemplateId,
                aggregate.Version,
                [EventFactory.Create(@event, "template.updated")],
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

    public async Task<CommandResult> DeleteAsync(DeleteTemplateCommand command, CancellationToken ct)
    {
        try
        {
            TemplateCommandValidator.Validate(command);

            var history = await _eventStore.LoadAggregateEventsAsync("template", command.TemplateId, ct);
            var aggregate = new TemplateAggregate();
            aggregate.LoadFrom(history);

            if (!aggregate.Exists || aggregate.IsDeleted)
                return CommandResult.NotFound("Template not found");

            if (command.ExpectedVersion != aggregate.Version)
                return CommandResult.Conflict($"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");

            var @event = aggregate.Delete();

            return await SaveAndPublishAsync(
                command.CommandId,
                "template",
                command.TemplateId,
                aggregate.Version,
                [EventFactory.Create(@event, "template.deleted")],
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
}