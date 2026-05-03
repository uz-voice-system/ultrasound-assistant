using UltrasoundAssistant.Contracts.Reads.Templates.Admin;
using UltrasoundAssistant.Contracts.Reads.Templates.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Templates;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

public interface ITemplateReadRepository
{
    Task<TemplateReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<TemplateReadModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TemplateReadModel>> SearchForDoctorAsync(TemplateSearchRequest filter, CancellationToken cancellationToken);

    Task<IReadOnlyList<TemplateReadModel>> SearchForAdminAsync(TemplateAdminSearchRequest filter, CancellationToken cancellationToken);

    Task AddAsync(TemplateReadModel template, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
