using UltrasoundAssistant.Contracts.Statistics;

namespace UltrasoundAssistant.ProjectionService.Application.Abstractions;

public interface IAdminStatisticsReadService
{
    Task<AdminStatisticsDto> GetAsync(AdminStatisticsRequest request, CancellationToken cancellationToken);
}