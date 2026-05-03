using UltrasoundAssistant.Contracts.Reads.Users.Details;
using UltrasoundAssistant.Contracts.Reads.Users.Search;

namespace UltrasoundAssistant.ProjectionService.Application.Abstractions;

public interface IUserReadService
{
    Task<UserDto?> GetByIdAsync(Guid id, bool includeInactive, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserSummaryDto>> SearchAsync(UserSearchRequest filter, CancellationToken cancellationToken);
}
