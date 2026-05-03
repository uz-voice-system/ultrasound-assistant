using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Reads.Schedules.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Repositories;

public sealed class UserScheduleReadRepository : IUserScheduleReadRepository
{
    private readonly ProjectionDbContext _dbContext;

    public UserScheduleReadRepository(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserScheduleReadModel?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var query = IncludeScheduleGraph(_dbContext.UserSchedules.AsNoTracking());

        if (!includeDeleted)
            query = query.Where(x => !x.IsDeleted);

        return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserScheduleReadModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await IncludeScheduleGraph(_dbContext.UserSchedules)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<UserScheduleReadModel>> GetByUserIdAsync(Guid userId, bool includeDeleted, CancellationToken cancellationToken)
    {
        var query = IncludeScheduleGraph(_dbContext.UserSchedules.AsNoTracking())
            .Where(x => x.UserId == userId);

        if (!includeDeleted)
            query = query.Where(x => !x.IsDeleted);

        return await query
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserScheduleReadModel>> GetByUserIdForUpdateAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.UserSchedules
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserScheduleReadModel>> SearchAsync(UserScheduleSearchRequest filter, CancellationToken cancellationToken)
    {
        var query = IncludeScheduleGraph(_dbContext.UserSchedules.AsNoTracking());

        if (!filter.IncludeDeleted)
            query = query.Where(x => !x.IsDeleted);

        if (filter.UserId is not null)
        {
            query = query.Where(x => x.UserId == filter.UserId.Value);
        }

        if (filter.UserRole is not null)
        {
            query = query.Where(x => x.User.Role == filter.UserRole.Value);
        }

        if (filter.DayOfWeek is not null)
        {
            query = query.Where(x => x.DayOfWeek == filter.DayOfWeek.Value);
        }

        return await query
            .OrderBy(x => x.User.FullName)
            .ThenBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserScheduleReadModel schedule, CancellationToken cancellationToken)
    {
        await _dbContext.UserSchedules.AddAsync(schedule, cancellationToken);
    }

    public async Task AddRangeAsync(IReadOnlyList<UserScheduleReadModel> schedules, CancellationToken cancellationToken)
    {
        await _dbContext.UserSchedules.AddRangeAsync(schedules, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<UserScheduleReadModel> IncludeScheduleGraph(IQueryable<UserScheduleReadModel> query)
    {
        return query.Include(x => x.User);
    }
}
