using System.Text.Json;
using UltrasoundAssistant.Contracts.Events.ReportEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Reports;

public sealed class ReportImageDeletedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;

    public string RoutingKey => "report.image.deleted";

    public ReportImageDeletedEventHandler(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<ReportImageDeletedEvent>(payload, JsonDefaults.Web)
            ?? throw new InvalidOperationException("Invalid ReportImageDeletedEvent payload");

        var report = await _dbContext.Reports.FindAsync(
            [@event.ReportId],
            cancellationToken);

        if (report is null)
            return;

        if (@event.Version <= report.Version)
            return;

        report.UltrasoundImageBytes = null;
        report.UltrasoundImageFileName = null;
        report.UltrasoundImageContentType = null;
        report.UltrasoundImageUploadedAtUtc = null;
        report.Version = @event.Version;
        report.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
