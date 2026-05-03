using UltrasoundAssistant.Contracts.Reads.Reports.Details;

namespace UltrasoundAssistant.ReportGenerator.Abstractions;

public interface IReportDataClient
{
    Task<ReportDto?> GetReportAsync(Guid reportId, CancellationToken ct);
}
