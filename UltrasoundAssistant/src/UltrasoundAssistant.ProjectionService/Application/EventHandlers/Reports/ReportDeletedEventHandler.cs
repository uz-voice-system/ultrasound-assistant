using UltrasoundAssistant.Contracts.Events.ReportEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Reports;

public sealed class ReportDeletedEventHandler : IIntegrationEventHandler
{
    private readonly IReportReadRepository _repository;

    public string RoutingKey => "report.deleted";

    public ReportDeletedEventHandler(IReportReadRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = EventPayloadReader.Read<ReportDeletedEvent>(payload, nameof(ReportDeletedEvent));

        var report = await _repository.GetByIdForUpdateAsync(
            @event.ReportId,
            cancellationToken);

        if (report is null)
            return;

        if (@event.Version <= report.Version)
            return;

        report.IsDeleted = true;
        report.UpdatedAtUtc = @event.UpdatedAtUtc;
        report.Version = @event.Version;

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
