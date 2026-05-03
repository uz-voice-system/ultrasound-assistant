using UltrasoundAssistant.Contracts.Reads.Reports.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

public interface IReportReadRepository
{
    Task<ReportReadModel?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken);

    Task<ReportReadModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<ReportReadModel?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ReportReadModel>> SearchAsync(ReportSearchRequest filter, CancellationToken cancellationToken);

    Task AddAsync(ReportReadModel report, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
