using UltrasoundAssistant.Contracts.Statistics;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

public interface IAdminStatisticsReadRepository
{
    Task<AdminStatisticsDto> GetAsync(AdminStatisticsRequest request, CancellationToken cancellationToken);
}