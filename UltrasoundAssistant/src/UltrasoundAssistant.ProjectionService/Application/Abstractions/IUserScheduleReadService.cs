using UltrasoundAssistant.Contracts.Reads.Schedules.Details;
using UltrasoundAssistant.Contracts.Reads.Schedules.Search;

namespace UltrasoundAssistant.ProjectionService.Application.Abstractions;

public interface IUserScheduleReadService
{
    Task<UserScheduleDto?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserScheduleDto>> GetByUserIdAsync(Guid userId, bool includeDeleted, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserScheduleSummaryDto>> SearchAsync(UserScheduleSearchRequest filter, CancellationToken cancellationToken);
}
