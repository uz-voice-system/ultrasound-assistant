using UltrasoundAssistant.Contracts.Reads.Appointments.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

public interface IAppointmentReadRepository
{
    Task<AppointmentReadModel?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken);

    Task<AppointmentReadModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<AppointmentReadModel>> SearchAsync(AppointmentSearchRequest filter, CancellationToken cancellationToken);

    Task AddAsync(AppointmentReadModel appointment, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
