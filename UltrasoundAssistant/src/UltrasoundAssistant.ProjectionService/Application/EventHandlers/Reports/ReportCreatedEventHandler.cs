using UltrasoundAssistant.Contracts.Events.ReportEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Reports;

public sealed class ReportCreatedEventHandler : IIntegrationEventHandler
{
    private readonly IReportReadRepository _repository;

    public string RoutingKey => "report.created";

    public ReportCreatedEventHandler(IReportReadRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = EventPayloadReader.Read<ReportCreatedEvent>(
            payload,
            nameof(ReportCreatedEvent));

        var report = await _repository.GetByIdForUpdateAsync(
            @event.ReportId,
            cancellationToken);

        if (report is not null && @event.Version <= report.Version)
            return;

        if (report is null)
        {
            report = new ReportReadModel
            {
                Id = @event.ReportId
            };

            await _repository.AddAsync(report, cancellationToken);
        }

        report.AppointmentId = @event.AppointmentId;
        report.Status = @event.Status;
        report.ContentJson = @event.ContentJson;
        report.IsDeleted = false;
        report.CreatedAtUtc = @event.CreatedAtUtc;
        report.UpdatedAtUtc = @event.UpdatedAtUtc;
        report.Version = @event.Version;

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
