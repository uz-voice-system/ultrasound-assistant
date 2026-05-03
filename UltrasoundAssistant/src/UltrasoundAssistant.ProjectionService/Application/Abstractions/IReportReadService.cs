using UltrasoundAssistant.Contracts.Reads.Reports.Details;
using UltrasoundAssistant.Contracts.Reads.Reports.Search;

namespace UltrasoundAssistant.ProjectionService.Application.Abstractions;

public interface IReportReadService
{
    Task<ReportDto?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken);

    Task<ReportDto?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ReportSummaryDto>> SearchAsync(ReportSearchRequest filter, CancellationToken cancellationToken);
}