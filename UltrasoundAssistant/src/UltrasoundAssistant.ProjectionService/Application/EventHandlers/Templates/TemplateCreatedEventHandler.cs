using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UltrasoundAssistant.Contracts.Events.TemplateEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Application.Mapping;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Templates;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Templates;

public sealed class TemplateCreatedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;
    private readonly TemplateProjectionMapper _mapper;

    public string RoutingKey => "template.created";

    public TemplateCreatedEventHandler(ProjectionDbContext dbContext, TemplateProjectionMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<TemplateCreatedEvent>(payload, JsonDefaults.Web)
            ?? throw new InvalidOperationException("Invalid TemplateCreatedEvent payload");

        var template = await LoadTemplateAsync(@event.TemplateId, cancellationToken);

        if (template is not null && @event.Version <= template.Version)
            return;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (template is null)
        {
            template = new TemplateReadModel
            {
                Id = @event.TemplateId
            };

            _dbContext.Templates.Add(template);
        }
        else
        {
            _dbContext.TemplateBlocks.RemoveRange(template.Blocks);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        template.Name = @event.Name;
        template.DefaultAppointmentDurationMinutes = @event.DefaultAppointmentDurationMinutes;
        template.IsDeleted = false;
        template.Version = @event.Version;
        template.Blocks = _mapper.MapBlocks(@event.TemplateId, @event.Blocks);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<TemplateReadModel?> LoadTemplateAsync(Guid templateId, CancellationToken cancellationToken)
    {
        return await _dbContext.Templates
            .Include(t => t.Blocks)
                .ThenInclude(b => b.Phrases)
            .Include(t => t.Blocks)
                .ThenInclude(b => b.Fields)
                    .ThenInclude(f => f.Phrases)
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);
    }
}
