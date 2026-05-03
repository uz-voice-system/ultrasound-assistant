using UltrasoundAssistant.Contracts.Reads.Schedules.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

public interface IUserScheduleReadRepository
{
    Task<UserScheduleReadModel?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken);

    Task<UserScheduleReadModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserScheduleReadModel>> GetByUserIdAsync(Guid userId, bool includeDeleted, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserScheduleReadModel>> GetByUserIdForUpdateAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserScheduleReadModel>> SearchAsync(UserScheduleSearchRequest filter, CancellationToken cancellationToken);

    Task AddAsync(UserScheduleReadModel schedule, CancellationToken cancellationToken);

    Task AddRangeAsync(IReadOnlyList<UserScheduleReadModel> schedules, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
