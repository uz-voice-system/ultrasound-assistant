using UltrasoundAssistant.Contracts.Reads.Reports;

namespace UltrasoundAssistant.ReportGenerator.Abstractions;

public interface IReportDataClient
{
    Task<ReportDto?> GetReportAsync(Guid reportId, CancellationToken ct);
}
