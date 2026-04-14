using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.ReportEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers;

public sealed class ReportCompletedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;
    private readonly ILogger<ReportCompletedEventHandler> _logger;

    public string RoutingKey => "report.completed";

    public ReportCompletedEventHandler(
        ProjectionDbContext dbContext,
        ILogger<ReportCompletedEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<ReportCompletedEvent>(payload, JsonDefaults.Web)
                     ?? throw new InvalidOperationException("Invalid ReportCompletedEvent payload");

        var report = await _dbContext.Reports
            .FirstOrDefaultAsync(x => x.Id == @event.ReportId, cancellationToken);

        if (report is null)
        {
            _logger.LogWarning("Report {ReportId} not found for report.completed", @event.ReportId);
            return;
        }

        if (@event.Version <= report.Version)
            return;

        report.Status = ReportStatus.Completed;
        report.UpdatedAtUtc = @event.CreatedAt;
        report.Version = @event.Version;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Projected report.completed for report {ReportId}", @event.ReportId);
    }
}