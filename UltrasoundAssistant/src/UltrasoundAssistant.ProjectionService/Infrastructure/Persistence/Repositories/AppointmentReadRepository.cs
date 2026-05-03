using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Reads.Appointments.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Repositories;

public sealed class AppointmentReadRepository : IAppointmentReadRepository
{
    private readonly ProjectionDbContext _dbContext;

    public AppointmentReadRepository(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppointmentReadModel?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var query = IncludeAppointmentGraph(_dbContext.Appointments.AsNoTracking());

        if (!includeDeleted)
            query = query.Where(x => !x.IsDeleted);

        return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<AppointmentReadModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await IncludeAppointmentGraph(_dbContext.Appointments).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentReadModel>> SearchAsync(AppointmentSearchRequest filter, CancellationToken cancellationToken)
    {
        var query = IncludeAppointmentGraph(_dbContext.Appointments.AsNoTracking());

        if (!filter.IncludeDeleted)
            query = query.Where(x => !x.IsDeleted);

        if (filter.PatientId is not null)
        {
            query = query.Where(x => x.PatientId == filter.PatientId.Value);
        }

        if (filter.DoctorId is not null)
        {
            query = query.Where(x => x.DoctorId == filter.DoctorId.Value);
        }

        if (filter.TemplateId is not null)
        {
            query = query.Where(x => x.TemplateId == filter.TemplateId.Value);
        }

        if (filter.CreatedByUserId is not null)
        {
            query = query.Where(x => x.CreatedByUserId == filter.CreatedByUserId.Value);
        }

        if (filter.Status is not null)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.FromUtc is not null)
        {
            query = query.Where(x => x.StartAtUtc >= filter.FromUtc.Value);
        }

        if (filter.ToUtc is not null)
        {
            query = query.Where(x => x.StartAtUtc <= filter.ToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = Normalize(filter.SearchText);

            query = query.Where(x =>
                x.Patient.FullName.ToLower().Contains(search) ||
                x.Doctor.FullName.ToLower().Contains(search) ||
                x.Template.Name.ToLower().Contains(search));
        }

        return await query
            .OrderBy(x => x.StartAtUtc)
            .ThenBy(x => x.Patient.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AppointmentReadModel appointment, CancellationToken cancellationToken)
    {
        await _dbContext.Appointments.AddAsync(appointment, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<AppointmentReadModel> IncludeAppointmentGraph(IQueryable<AppointmentReadModel> query)
    {
        return query
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .Include(x => x.CreatedByUser)
            .Include(x => x.Template)
            .Include(x => x.Report);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLower();
    }
}
