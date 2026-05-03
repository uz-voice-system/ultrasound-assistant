using UltrasoundAssistant.Contracts.Reads.Patients.Details;
using UltrasoundAssistant.Contracts.Reads.Patients.Search;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Mapping;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Application.Services;

public sealed class PatientReadService : IPatientReadService
{
    private readonly IPatientReadRepository _repository;
    private readonly PatientProjectionMapper _mapper;

    public PatientReadService(IPatientReadRepository repository, PatientProjectionMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PatientDto?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var patient = await _repository.GetByIdAsync(id, includeDeleted, cancellationToken);

        return patient is null ? null : _mapper.MapFull(patient);
    }

    public async Task<IReadOnlyList<PatientSummaryDto>> SearchAsync(PatientSearchRequest filter, CancellationToken cancellationToken)
    {
        var patients = await _repository.SearchAsync(filter, cancellationToken);

        return patients.Select(_mapper.MapSummary).ToList();
    }
}