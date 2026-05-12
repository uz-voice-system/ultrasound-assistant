using System.Text.Json;
using UltrasoundAssistant.Contracts.Events.ReportEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Reports;

public sealed class ReportImageUploadedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;

    public string RoutingKey => "report.image.uploaded";

    public ReportImageUploadedEventHandler(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<ReportImageUploadedEvent>(payload, JsonDefaults.Web)
            ?? throw new InvalidOperationException("Invalid ReportImageUploadedEvent payload");

        var report = await _dbContext.Reports.FindAsync(
            [@event.ReportId],
            cancellationToken);

        if (report is null)
            return;

        if (@event.Version <= report.Version)
            return;

        report.UltrasoundImageBytes = Convert.FromBase64String(@event.ImageBase64);
        report.UltrasoundImageFileName = @event.FileName;
        report.UltrasoundImageContentType = @event.ContentType;
        report.UltrasoundImageUploadedAtUtc = @event.UploadedAtUtc;
        report.Version = @event.Version;
        report.UpdatedAtUtc = @event.UploadedAtUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
