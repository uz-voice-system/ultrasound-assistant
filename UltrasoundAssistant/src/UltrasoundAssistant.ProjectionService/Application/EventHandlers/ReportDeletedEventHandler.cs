using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Events.ReportEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers;

public sealed class ReportDeletedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;
    private readonly ILogger<ReportDeletedEventHandler> _logger;

    public string RoutingKey => "report.deleted";

    public ReportDeletedEventHandler(
        ProjectionDbContext dbContext,
        ILogger<ReportDeletedEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<ReportDeletedEvent>(payload, JsonDefaults.Web)
                     ?? throw new InvalidOperationException("Invalid ReportDeletedEvent payload");

        var report = await _dbContext.Reports
            .FirstOrDefaultAsync(x => x.Id == @event.ReportId, cancellationToken);

        if (report is null)
        {
            _logger.LogWarning("Report {ReportId} not found for report.deleted", @event.ReportId);
            return;
        }

        if (@event.Version <= report.Version)
            return;

        report.IsDeleted = true;
        report.UpdatedAtUtc = @event.CreatedAt;
        report.Version = @event.Version;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Projected report.deleted for report {ReportId}", @event.ReportId);
    }
}