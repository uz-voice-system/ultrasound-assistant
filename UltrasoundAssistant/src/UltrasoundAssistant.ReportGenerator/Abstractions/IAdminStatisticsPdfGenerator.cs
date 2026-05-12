using UltrasoundAssistant.Contracts.Statistics;

namespace UltrasoundAssistant.ReportGenerator.Abstractions;

public interface IAdminStatisticsPdfGenerator
{
    byte[] Generate(AdminStatisticsDto statistics);
}
