using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Reads.Reports.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Repositories;

public sealed class ReportReadRepository : IReportReadRepository
{
    private readonly ProjectionDbContext _dbContext;

    public ReportReadRepository(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReportReadModel?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var query = IncludeReportGraph(_dbContext.Reports.AsNoTracking());

        if (!includeDeleted)
            query = query.Where(x => !x.IsDeleted);

        return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ReportReadModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await IncludeReportGraph(_dbContext.Reports)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ReportReadModel?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        return await IncludeReportGraph(_dbContext.Reports.AsNoTracking())
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<ReportReadModel>> SearchAsync(ReportSearchRequest filter, CancellationToken cancellationToken)
    {
        var query = IncludeReportGraph(_dbContext.Reports.AsNoTracking());

        if (!filter.IncludeDeleted)
            query = query.Where(x => !x.IsDeleted);

        if (filter.AppointmentId is not null)
        {
            query = query.Where(x => x.AppointmentId == filter.AppointmentId.Value);
        }

        if (filter.PatientId is not null)
        {
            query = query.Where(x => x.Appointment.PatientId == filter.PatientId.Value);
        }

        if (filter.DoctorId is not null)
        {
            query = query.Where(x => x.Appointment.DoctorId == filter.DoctorId.Value);
        }

        if (filter.TemplateId is not null)
        {
            query = query.Where(x => x.Appointment.TemplateId == filter.TemplateId.Value);
        }

        if (filter.Status is not null)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.CreatedFromUtc is not null)
        {
            query = query.Where(x => x.CreatedAtUtc >= filter.CreatedFromUtc.Value);
        }

        if (filter.CreatedToUtc is not null)
        {
            query = query.Where(x => x.CreatedAtUtc <= filter.CreatedToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = Normalize(filter.SearchText);

            query = query.Where(x =>
                x.Appointment.Patient.FullName.ToLower().Contains(search) ||
                x.Appointment.Doctor.FullName.ToLower().Contains(search) ||
                x.Appointment.Template.Name.ToLower().Contains(search));
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ReportReadModel report, CancellationToken cancellationToken)
    {
        await _dbContext.Reports.AddAsync(report, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<ReportReadModel> IncludeReportGraph(IQueryable<ReportReadModel> query)
    {
        return query
            .Include(x => x.Appointment)
                .ThenInclude(x => x.Patient)
            .Include(x => x.Appointment)
                .ThenInclude(x => x.Doctor)
            .Include(x => x.Appointment)
                .ThenInclude(x => x.Template);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLower();
    }
}
