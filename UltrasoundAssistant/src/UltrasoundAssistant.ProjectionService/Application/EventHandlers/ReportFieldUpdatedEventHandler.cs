using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Events.ReportEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers;

public sealed class ReportFieldUpdatedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;
    private readonly ILogger<ReportFieldUpdatedEventHandler> _logger;

    public string RoutingKey => "report.field.updated";

    public ReportFieldUpdatedEventHandler(
        ProjectionDbContext dbContext,
        ILogger<ReportFieldUpdatedEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<ReportFieldUpdatedEvent>(payload, JsonDefaults.Web)
                     ?? throw new InvalidOperationException("Invalid ReportFieldUpdatedEvent payload");

        var report = await _dbContext.Reports
            .FirstOrDefaultAsync(x => x.Id == @event.ReportId, cancellationToken);

        if (report is null)
        {
            _logger.LogWarning("Report {ReportId} not found for report.field.updated", @event.ReportId);
            return;
        }

        if (@event.Version <= report.Version)
            return;

        var content = string.IsNullOrWhiteSpace(report.ContentJson)
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : JsonSerializer.Deserialize<Dictionary<string, string>>(report.ContentJson, JsonDefaults.Web)
              ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        content[@event.FieldName] = @event.Value;

        report.ContentJson = JsonSerializer.Serialize(content, JsonDefaults.Web);
        report.UpdatedAtUtc = @event.CreatedAt;
        report.Version = @event.Version;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Projected report.field.updated for report {ReportId}, field {FieldName}",
            @event.ReportId,
            @event.FieldName);
    }
}