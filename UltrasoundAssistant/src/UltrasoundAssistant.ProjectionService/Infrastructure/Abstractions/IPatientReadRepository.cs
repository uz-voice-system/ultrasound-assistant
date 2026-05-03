using UltrasoundAssistant.Contracts.Reads.Patients.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Patients;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

public interface IPatientReadRepository
{
    Task<PatientReadModel?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken);

    Task<PatientReadModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<PatientReadModel>> SearchAsync(PatientSearchRequest filter, CancellationToken cancellationToken);

    Task AddAsync(PatientReadModel patient, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
