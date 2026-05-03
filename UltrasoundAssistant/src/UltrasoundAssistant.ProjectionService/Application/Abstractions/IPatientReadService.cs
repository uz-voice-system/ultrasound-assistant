using UltrasoundAssistant.Contracts.Reads.Patients.Details;
using UltrasoundAssistant.Contracts.Reads.Patients.Search;

namespace UltrasoundAssistant.ProjectionService.Application.Abstractions;

public interface IPatientReadService
{
    Task<PatientDto?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken);

    Task<IReadOnlyList<PatientSummaryDto>> SearchAsync(PatientSearchRequest filter, CancellationToken cancellationToken);
}
