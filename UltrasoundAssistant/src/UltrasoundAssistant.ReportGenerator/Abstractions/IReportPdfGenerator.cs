using UltrasoundAssistant.Contracts.Reads.Reports.Details;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;

namespace UltrasoundAssistant.ReportGenerator.Abstractions;

public interface IReportPdfGenerator
{
    byte[] Generate(ReportDto report, TemplateDto template);
}
