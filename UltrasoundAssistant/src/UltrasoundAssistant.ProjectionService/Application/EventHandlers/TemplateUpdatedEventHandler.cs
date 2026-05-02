using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UltrasoundAssistant.Contracts.Events.TemplateEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Application.Mapping;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers;

public sealed class TemplateUpdatedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;
    private readonly TemplateProjectionMapper _mapper;

    public string RoutingKey => "template.updated";

    public TemplateUpdatedEventHandler(
        ProjectionDbContext dbContext,
        TemplateProjectionMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<TemplateUpdatedEvent>(payload, JsonDefaults.Web)
            ?? throw new InvalidOperationException("Invalid TemplateUpdatedEvent payload");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var template = await _dbContext.Templates
            .FirstOrDefaultAsync(t => t.Id == @event.TemplateId, cancellationToken);

        if (template is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        if (@event.Version <= template.Version)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        if (!string.IsNullOrWhiteSpace(@event.Name))
            template.Name = @event.Name;

        if (@event.Blocks is not null)
        {
            await DeleteOldTemplateStructureAsync(@event.TemplateId, cancellationToken);

            var newBlocks = _mapper.MapBlocks(template.Id, @event.Blocks);

            _dbContext.TemplateBlocks.AddRange(newBlocks);
        }

        template.IsDeleted = false;
        template.Version = @event.Version;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task DeleteOldTemplateStructureAsync(Guid templateId, CancellationToken cancellationToken)
    {
        await _dbContext.TemplateFieldPhrases
            .Where(x => x.Field.Block.TemplateId == templateId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.TemplateBlockPhrases
            .Where(x => x.Block.TemplateId == templateId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.TemplateFields
            .Where(x => x.Block.TemplateId == templateId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.TemplateBlocks
            .Where(x => x.TemplateId == templateId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}