using UltrasoundAssistant.Contracts.Reads.Users.Details;
using UltrasoundAssistant.Contracts.Reads.Users.Search;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Mapping;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Application.Services;

public sealed class UserReadService : IUserReadService
{
    private readonly IUserReadRepository _repository;
    private readonly UserProjectionMapper _mapper;

    public UserReadService(IUserReadRepository repository, UserProjectionMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, bool includeInactive, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(id, includeInactive, cancellationToken);

        return user is null ? null : _mapper.MapFull(user);
    }

    public async Task<IReadOnlyList<UserSummaryDto>> SearchAsync(UserSearchRequest filter, CancellationToken cancellationToken)
    {
        var users = await _repository.SearchAsync(filter, cancellationToken);

        return users.Select(_mapper.MapSummary).ToList();
    }
}
