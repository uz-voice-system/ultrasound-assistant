using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Events.ReportEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers;

public sealed class ReportCreatedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;
    private readonly ILogger<ReportCreatedEventHandler> _logger;

    public string RoutingKey => "report.created";

    public ReportCreatedEventHandler(
        ProjectionDbContext dbContext,
        ILogger<ReportCreatedEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<ReportCreatedEvent>(payload, JsonDefaults.Web)
                     ?? throw new InvalidOperationException("Invalid ReportCreatedEvent payload");

        var existing = await _dbContext.Reports
            .FirstOrDefaultAsync(x => x.Id == @event.Id, cancellationToken);

        if (existing is not null && @event.Version <= existing.Version)
            return;

        if (existing is null)
        {
            _dbContext.Reports.Add(new ReportReadModel
            {
                Id = @event.Id,
                PatientId = @event.PatientId,
                DoctorId = @event.DoctorId,
                TemplateId = @event.TemplateId,
                Status = @event.Status,
                ContentJson = "{}",
                IsDeleted = false,
                CreatedAtUtc = @event.CreatedAt,
                UpdatedAtUtc = @event.CreatedAt,
                Version = @event.Version
            });
        }
        else
        {
            existing.PatientId = @event.PatientId;
            existing.DoctorId = @event.DoctorId;
            existing.TemplateId = @event.TemplateId;
            existing.Status = @event.Status;
            existing.IsDeleted = false;
            existing.UpdatedAtUtc = @event.CreatedAt;
            existing.Version = @event.Version;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Projected report.created for report {ReportId}", @event.Id);
    }
}