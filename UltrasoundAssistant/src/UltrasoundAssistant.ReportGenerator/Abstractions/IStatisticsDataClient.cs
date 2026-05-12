using UltrasoundAssistant.Contracts.Statistics;

namespace UltrasoundAssistant.ReportGenerator.Abstractions;

public interface IStatisticsDataClient
{
    Task<AdminStatisticsDto?> GetAdminStatisticsAsync(AdminStatisticsRequest request, CancellationToken cancellationToken);
}