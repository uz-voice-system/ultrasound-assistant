using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Reads.Patients.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Patients;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Repositories;

public sealed class PatientReadRepository : IPatientReadRepository
{
    private readonly ProjectionDbContext _dbContext;

    public PatientReadRepository(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PatientReadModel?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var query = IncludePatientGraph(_dbContext.Patients.AsNoTracking());

        if (!includeDeleted)
            query = query.Where(x => !x.IsDeleted);

        return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PatientReadModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await IncludePatientGraph(_dbContext.Patients)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PatientReadModel>> SearchAsync(PatientSearchRequest filter, CancellationToken cancellationToken)
    {
        var query = _dbContext.Patients.AsNoTracking();

        if (!filter.IncludeDeleted)
            query = query.Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = Normalize(filter.SearchText);

            query = query.Where(x =>
                x.FullName.ToLower().Contains(search) ||
                (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(search)) ||
                (x.Email != null && x.Email.ToLower().Contains(search)) ||
                x.Documents.Any(d =>
                    d.Number.ToLower().Contains(search) ||
                    (d.Series != null && d.Series.ToLower().Contains(search))));
        }

        if (!string.IsNullOrWhiteSpace(filter.FullName))
        {
            var fullName = Normalize(filter.FullName);

            query = query.Where(x => x.FullName.ToLower().Contains(fullName));
        }

        if (filter.BirthDate is not null)
        {
            var birthDate = filter.BirthDate.Value.Date;

            query = query.Where(x => x.BirthDate.Date == birthDate);
        }

        if (filter.DocumentType is not null)
        {
            query = query.Where(x => x.Documents.Any(d => d.DocumentType == filter.DocumentType.Value));
        }

        if (!string.IsNullOrWhiteSpace(filter.DocumentSeries))
        {
            var documentSeries = Normalize(filter.DocumentSeries);

            query = query.Where(x =>
                x.Documents.Any(d =>
                    d.Series != null &&
                    d.Series.ToLower().Contains(documentSeries)));
        }

        if (!string.IsNullOrWhiteSpace(filter.DocumentNumber))
        {
            var documentNumber = Normalize(filter.DocumentNumber);

            query = query.Where(x =>
                x.Documents.Any(d =>
                    d.Number.ToLower().Contains(documentNumber)));
        }

        if (!string.IsNullOrWhiteSpace(filter.PhoneNumber))
        {
            var phoneNumber = Normalize(filter.PhoneNumber);

            query = query.Where(x =>
                x.PhoneNumber != null &&
                x.PhoneNumber.ToLower().Contains(phoneNumber));
        }

        return await query
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.BirthDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PatientReadModel patient, CancellationToken cancellationToken)
    {
        await _dbContext.Patients.AddAsync(patient, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<PatientReadModel> IncludePatientGraph(
        IQueryable<PatientReadModel> query)
    {
        return query.Include(x => x.Documents);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLower();
    }
}
