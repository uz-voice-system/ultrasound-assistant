using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Reads.Users.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Repositories;

public sealed class UserReadRepository : IUserReadRepository
{
    private readonly ProjectionDbContext _dbContext;

    public UserReadRepository(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserReadModel?> GetByIdAsync(Guid id, bool includeInactive, CancellationToken cancellationToken)
    {
        var query = IncludeUserGraph(_dbContext.Users.AsNoTracking());

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserReadModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await IncludeUserGraph(_dbContext.Users)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserReadModel?> GetByLoginAsync(string login, CancellationToken cancellationToken)
    {
        var normalizedLogin = Normalize(login);

        return await IncludeUserGraph(_dbContext.Users.AsNoTracking())
            .FirstOrDefaultAsync(x =>
                x.Login.ToLower() == normalizedLogin,
                cancellationToken);
    }

    public async Task<IReadOnlyList<UserReadModel>> SearchAsync(UserSearchRequest filter, CancellationToken cancellationToken)
    {
        var query = IncludeUserGraph(_dbContext.Users.AsNoTracking());

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = Normalize(filter.SearchText);

            query = query.Where(x =>
                x.Login.ToLower().Contains(search) ||
                x.FullName.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Login))
        {
            var login = Normalize(filter.Login);

            query = query.Where(x => x.Login.ToLower().Contains(login));
        }

        if (!string.IsNullOrWhiteSpace(filter.FullName))
        {
            var fullName = Normalize(filter.FullName);

            query = query.Where(x => x.FullName.ToLower().Contains(fullName));
        }

        if (filter.Role is not null)
        {
            query = query.Where(x => x.Role == filter.Role.Value);
        }

        if (filter.IsActive is not null)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        return await query
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.Login)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserReadModel user, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<UserReadModel> IncludeUserGraph(IQueryable<UserReadModel> query)
    {
        return query.Include(x => x.DoctorProfile);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLower();
    }
}
