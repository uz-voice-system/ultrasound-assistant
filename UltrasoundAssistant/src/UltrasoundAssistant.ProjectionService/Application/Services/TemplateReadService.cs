using UltrasoundAssistant.Contracts.Reads.Templates.Admin;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.Contracts.Reads.Templates.Search;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Mapping;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Application.Services;

public sealed class TemplateReadService : ITemplateReadService
{
    private readonly ITemplateReadRepository _repository;
    private readonly TemplateProjectionMapper _mapper;

    public TemplateReadService(ITemplateReadRepository repository, TemplateProjectionMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TemplateDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(id, cancellationToken);
        return template is null ? null : _mapper.MapFull(template);
    }

    public async Task<IReadOnlyList<TemplateSummaryDto>> SearchForDoctorAsync(TemplateSearchRequest filter, CancellationToken cancellationToken)
    {
        var templates = await _repository.SearchForDoctorAsync(filter, cancellationToken);
        return templates.Select(_mapper.MapSummary).ToList();
    }

    public async Task<IReadOnlyList<TemplateAdminSearchResultDto>> SearchForAdminAsync(TemplateAdminSearchRequest filter, CancellationToken cancellationToken)
    {
        var templates = await _repository.SearchForAdminAsync(filter, cancellationToken);
        return templates.Select(template => _mapper.MapAdminSearchResult(template, filter)).ToList();
    }
}
