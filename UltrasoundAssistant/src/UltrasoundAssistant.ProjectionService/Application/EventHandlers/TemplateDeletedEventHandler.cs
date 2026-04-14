using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UltrasoundAssistant.Contracts.Events.TemplateEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers;

public sealed class TemplateDeletedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;

    public string RoutingKey => "template.deleted";

    public TemplateDeletedEventHandler(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<TemplateDeletedEvent>(payload, JsonDefaults.Web)
                     ?? throw new InvalidOperationException("Invalid TemplateDeletedEvent payload");

        var template = await _dbContext.Templates
            .FirstOrDefaultAsync(x => x.Id == @event.TemplateId, cancellationToken);

        if (template is null)
            return;

        if (@event.Version <= template.Version)
            return;

        template.IsDeleted = true;
        template.Version = @event.Version;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
