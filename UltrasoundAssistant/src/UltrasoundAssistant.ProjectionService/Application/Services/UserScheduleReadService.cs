using UltrasoundAssistant.Contracts.Reads.Schedules.Details;
using UltrasoundAssistant.Contracts.Reads.Schedules.Search;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Mapping;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Application.Services;

public sealed class UserScheduleReadService : IUserScheduleReadService
{
    private readonly IUserScheduleReadRepository _repository;
    private readonly UserScheduleProjectionMapper _mapper;

    public UserScheduleReadService(
        IUserScheduleReadRepository repository,
        UserScheduleProjectionMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserScheduleDto?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var schedule = await _repository.GetByIdAsync(id, includeDeleted, cancellationToken);

        return schedule is null ? null : _mapper.MapFull(schedule);
    }

    public async Task<IReadOnlyList<UserScheduleDto>> GetByUserIdAsync(Guid userId, bool includeDeleted, CancellationToken cancellationToken)
    {
        var schedules = await _repository.GetByUserIdAsync(userId, includeDeleted, cancellationToken);

        return schedules.Select(_mapper.MapFull).ToList();
    }

    public async Task<IReadOnlyList<UserScheduleSummaryDto>> SearchAsync(UserScheduleSearchRequest filter, CancellationToken cancellationToken)
    {
        var schedules = await _repository.SearchAsync(filter, cancellationToken);

        return schedules.Select(_mapper.MapSummary).ToList();
    }
}