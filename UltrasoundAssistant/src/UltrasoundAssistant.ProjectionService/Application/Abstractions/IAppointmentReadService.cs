using UltrasoundAssistant.Contracts.Reads.Appointments.Details;
using UltrasoundAssistant.Contracts.Reads.Appointments.Search;

namespace UltrasoundAssistant.ProjectionService.Application.Abstractions;

public interface IAppointmentReadService
{
    Task<AppointmentDto?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken);

    Task<IReadOnlyList<AppointmentSummaryDto>> SearchAsync(AppointmentSearchRequest filter, CancellationToken cancellationToken);
}
