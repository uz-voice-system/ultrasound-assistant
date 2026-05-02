using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Reads.Templates.Admin;
using UltrasoundAssistant.Contracts.Reads.Templates.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Templates;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Repositories;

public sealed class TemplateReadRepository : ITemplateReadRepository
{
    private readonly ProjectionDbContext _dbContext;

    public TemplateReadRepository(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TemplateReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = IncludeTemplateGraph(_dbContext.Templates.AsNoTracking());
        return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TemplateReadModel>> SearchForDoctorAsync(TemplateSearchRequest filter, CancellationToken cancellationToken)
    {
        var query = _dbContext.Templates.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var searchText = filter.SearchText.Trim().ToLower();

            query = query.Where(x => x.Name.ToLower().Contains(searchText));
        }

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TemplateReadModel>> SearchForAdminAsync(TemplateAdminSearchRequest filter, CancellationToken cancellationToken)
    {
        var query = IncludeTemplateGraph(_dbContext.Templates.AsNoTracking());

        if (!filter.IncludeDeleted)
            query = query.Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            query = ApplyGeneralSearch(query, filter.SearchText);

        if (!string.IsNullOrWhiteSpace(filter.TemplateName))
        {
            var templateName = Normalize(filter.TemplateName);

            query = query.Where(x =>
                x.Name.ToLower().Contains(templateName));
        }

        if (!string.IsNullOrWhiteSpace(filter.BlockName))
        {
            var blockName = Normalize(filter.BlockName);

            query = query.Where(x =>
                x.Blocks.Any(b => b.Name.ToLower().Contains(blockName)));
        }

        if (!string.IsNullOrWhiteSpace(filter.FieldName))
        {
            var fieldName = Normalize(filter.FieldName);

            query = query.Where(x =>
                x.Blocks.Any(b =>
                    b.Fields.Any(f => f.FieldName.ToLower().Contains(fieldName))));
        }

        if (!string.IsNullOrWhiteSpace(filter.FieldDisplayName))
        {
            var fieldDisplayName = Normalize(filter.FieldDisplayName);

            query = query.Where(x =>
                x.Blocks.Any(b =>
                    b.Fields.Any(f => f.DisplayName.ToLower().Contains(fieldDisplayName))));
        }

        if (!string.IsNullOrWhiteSpace(filter.Phrase))
        {
            var phrase = Normalize(filter.Phrase);

            query = query.Where(x =>
                x.Blocks.Any(b =>
                    b.Phrases.Any(p => p.Phrase.ToLower().Contains(phrase)) ||
                    b.Fields.Any(f =>
                        f.Phrases.Any(p => p.Phrase.ToLower().Contains(phrase)))));
        }

        if (filter.FieldType is not null)
        {
            query = query.Where(x =>
                x.Blocks.Any(b =>
                    b.Fields.Any(f => f.Type == filter.FieldType.Value)));
        }

        if (filter.HasNorm is not null)
        {
            if (filter.HasNorm.Value)
            {
                query = query.Where(x =>
                    x.Blocks.Any(b =>
                        b.Fields.Any(f =>
                            f.NormMin != null ||
                            f.NormMax != null ||
                            f.NormUnit != null ||
                            f.NormNormalText != null)));
            }
            else
            {
                query = query.Where(x =>
                    x.Blocks.Any(b =>
                        b.Fields.Any(f =>
                            f.NormMin == null &&
                            f.NormMax == null &&
                            f.NormUnit == null &&
                            f.NormNormalText == null)));
            }
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<TemplateReadModel> ApplyGeneralSearch(
        IQueryable<TemplateReadModel> query,
        string searchText)
    {
        var search = Normalize(searchText);

        return query.Where(x =>
            x.Name.ToLower().Contains(search) ||
            x.Blocks.Any(b =>
                b.Name.ToLower().Contains(search) ||
                b.Phrases.Any(p => p.Phrase.ToLower().Contains(search)) ||
                b.Fields.Any(f =>
                    f.FieldName.ToLower().Contains(search) ||
                    f.DisplayName.ToLower().Contains(search) ||
                    f.Phrases.Any(p => p.Phrase.ToLower().Contains(search)))));
    }

    private static IQueryable<TemplateReadModel> IncludeTemplateGraph(
        IQueryable<TemplateReadModel> query)
    {
        return query
            .Include(t => t.Blocks)
                .ThenInclude(b => b.Phrases)
            .Include(t => t.Blocks)
                .ThenInclude(b => b.Fields)
                    .ThenInclude(f => f.Phrases);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLower();
    }
}
