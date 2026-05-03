using UltrasoundAssistant.Contracts.Reads.Appointments.Details;
using UltrasoundAssistant.Contracts.Reads.Appointments.Search;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Mapping;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Application.Services;

public sealed class AppointmentReadService : IAppointmentReadService
{
    private readonly IAppointmentReadRepository _repository;
    private readonly AppointmentProjectionMapper _mapper;

    public AppointmentReadService(
        IAppointmentReadRepository repository,
        AppointmentProjectionMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<AppointmentDto?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var appointment = await _repository.GetByIdAsync(id, includeDeleted, cancellationToken);

        return appointment is null ? null : _mapper.MapFull(appointment);
    }

    public async Task<IReadOnlyList<AppointmentSummaryDto>> SearchAsync(AppointmentSearchRequest filter, CancellationToken cancellationToken)
    {
        var appointments = await _repository.SearchAsync(filter, cancellationToken);

        return appointments.Select(_mapper.MapSummary).ToList();
    }
}
