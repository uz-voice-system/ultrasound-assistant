using UltrasoundAssistant.Contracts.Reads.Templates.Admin;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.Contracts.Reads.Templates.Search;

namespace UltrasoundAssistant.ProjectionService.Application.Abstractions;

public interface ITemplateReadService
{
    Task<TemplateDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TemplateSummaryDto>> SearchForDoctorAsync(TemplateSearchRequest filter, CancellationToken cancellationToken);

    Task<IReadOnlyList<TemplateSummaryDto>> SearchForAdminAsync(TemplateAdminSearchRequest filter, CancellationToken cancellationToken);
}
