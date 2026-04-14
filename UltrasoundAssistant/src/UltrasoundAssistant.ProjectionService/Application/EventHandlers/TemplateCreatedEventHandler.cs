using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UltrasoundAssistant.Contracts.Events.TemplateEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers;

public sealed class TemplateCreatedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;

    public string RoutingKey => "template.created";

    public TemplateCreatedEventHandler(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<TemplateCreatedEvent>(payload, JsonDefaults.Web)
                     ?? throw new InvalidOperationException("Invalid TemplateCreatedEvent payload");

        var existing = await _dbContext.Templates
            .Include(x => x.Keywords)
            .FirstOrDefaultAsync(x => x.Id == @event.TemplateId, cancellationToken);

        if (existing is not null && @event.Version <= existing.Version)
            return;

        if (existing is null)
        {
            existing = new TemplateReadModel
            {
                Id = @event.TemplateId
            };
            _dbContext.Templates.Add(existing);
        }
        else
        {
            _dbContext.TemplateKeywords.RemoveRange(existing.Keywords);
        }

        existing.Name = @event.Name;
        existing.StructureJson = JsonSerializer.Serialize(@event.Keywords.Keys.OrderBy(x => x));
        existing.IsDeleted = false;
        existing.Version = @event.Version;
        existing.Keywords = @event.Keywords
            .Select(kvp => new TemplateKeywordReadModel
            {
                TemplateId = @event.TemplateId,
                Phrase = kvp.Key,
                TargetField = kvp.Value
            })
            .ToList();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}