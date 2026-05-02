using UltrasoundAssistant.Contracts.Reads.Templates.Details;

namespace UltrasoundAssistant.ReportGenerator.Abstractions;

public interface ITemplateClient
{
    Task<TemplateDto?> GetAsync(Guid templateId, CancellationToken ct);
}
