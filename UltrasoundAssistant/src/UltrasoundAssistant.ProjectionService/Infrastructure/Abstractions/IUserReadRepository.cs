using UltrasoundAssistant.Contracts.Reads.Users.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

public interface IUserReadRepository
{
    Task<UserReadModel?> GetByIdAsync(Guid id, bool includeInactive, CancellationToken cancellationToken);

    Task<UserReadModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<UserReadModel?> GetByLoginAsync(string login, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserReadModel>> SearchAsync(UserSearchRequest filter, CancellationToken cancellationToken);

    Task AddAsync(UserReadModel user, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
