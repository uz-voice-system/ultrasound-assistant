using Microsoft.AspNetCore.Mvc;
using UltrasoundAssistant.ReportGenerator.Abstractions;
using UltrasoundAssistant.ReportGenerator.Services;

namespace UltrasoundAssistant.ReportGenerator.Controllers;

[ApiController]
[Route("api/generated-reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportDataClient _reportDataClient;
    private readonly ITemplateClient _templateClient;
    private readonly IReportPdfGenerator _pdfGenerator;

    public ReportsController(
        IReportDataClient reportDataClient,
        IReportPdfGenerator pdfGenerator,
        ITemplateClient templateClient)
    {
        _reportDataClient = reportDataClient;
        _pdfGenerator = pdfGenerator;
        _templateClient = templateClient;
    }

    [HttpGet("{reportId:guid}/pdf")]
    public async Task<IActionResult> GeneratePdf(Guid reportId, CancellationToken ct)
    {
        var report = await _reportDataClient.GetReportAsync(reportId, ct);
        if (report is null)
            return NotFound(new { message = "Report not found" });

        var template = await _templateClient.GetAsync(report.TemplateId, ct);
        if (template is null)
            return NotFound(new { message = "Template not found" });

        var pdf = _pdfGenerator.Generate(report, template);
        var fileName = $"report-{reportId:N}.pdf";

        return File(pdf, "application/pdf", fileName);
    }
}
