using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.Contracts.Reads.Templates.Admin;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.Contracts.Reads.Templates.Search;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Controllers;

[ApiController]
[Route("api/read/templates")]
public sealed class TemplatesReadController : ControllerBase
{
    private readonly ITemplateReadService _templateReadService;

    public TemplatesReadController(ITemplateReadService templateReadService)
    {
        _templateReadService = templateReadService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TemplateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var template = await _templateReadService.GetByIdAsync(id, cancellationToken);

        if (template is null)
            return NotFound();

        return Ok(template);
    }

    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<TemplateSummaryDto>>> SearchForDoctor([FromBody] TemplateSearchRequest filter, CancellationToken cancellationToken)
    {
        var templates = await _templateReadService.SearchForDoctorAsync(filter, cancellationToken);

        return Ok(templates);
    }

    [HttpPost("search-admin")]
    public async Task<ActionResult<IReadOnlyList<TemplateSummaryDto>>> SearchForAdmin([FromBody] TemplateAdminSearchRequest filter, CancellationToken cancellationToken)
    {
        var templates = await _templateReadService.SearchForAdminAsync(filter, cancellationToken);

        return Ok(templates);
    }
}
