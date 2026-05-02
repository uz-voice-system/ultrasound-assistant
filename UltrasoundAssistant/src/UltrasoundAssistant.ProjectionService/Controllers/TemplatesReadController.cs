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

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<TemplateSummaryDto>>> SearchForDoctor([FromQuery] TemplateSearchRequest filter, CancellationToken cancellationToken)
    {
        var templates = await _templateReadService.SearchForDoctorAsync(filter, cancellationToken);

        return Ok(templates);
    }

    [HttpGet("search-admin")]
    public async Task<ActionResult<IReadOnlyList<TemplateSummaryDto>>> SearchForAdmin([FromQuery] TemplateAdminSearchRequest filter, CancellationToken cancellationToken)
    {
        var templates = await _templateReadService.SearchForAdminAsync(filter, cancellationToken);

        return Ok(templates);
    }
}
