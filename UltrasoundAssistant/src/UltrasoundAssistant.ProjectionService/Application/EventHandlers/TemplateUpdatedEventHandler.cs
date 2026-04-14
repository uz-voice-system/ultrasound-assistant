using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UltrasoundAssistant.Contracts.Events.TemplateEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers;

public sealed class TemplateUpdatedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;

    public string RoutingKey => "template.updated";

    public TemplateUpdatedEventHandler(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<TemplateUpdatedEvent>(payload, JsonDefaults.Web)
                     ?? throw new InvalidOperationException("Invalid TemplateUpdatedEvent payload");

        var template = await _dbContext.Templates
            .Include(x => x.Keywords)
            .FirstOrDefaultAsync(x => x.Id == @event.TemplateId, cancellationToken);

        if (template is null)
            return;

        if (@event.Version <= template.Version)
            return;

        if (!string.IsNullOrWhiteSpace(@event.Name))
            template.Name = @event.Name;

        if (@event.Keywords is not null)
        {
            _dbContext.TemplateKeywords.RemoveRange(template.Keywords);

            template.Keywords = @event.Keywords
                .OrderBy(x => x.Key)
                .Select(kvp => new TemplateKeywordReadModel
                {
                    TemplateId = template.Id,
                    Phrase = kvp.Key,
                    TargetField = kvp.Value
                })
                .ToList();

            template.StructureJson = JsonSerializer.Serialize(
                @event.Keywords.Keys.OrderBy(x => x));
        }

        template.Version = @event.Version;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
