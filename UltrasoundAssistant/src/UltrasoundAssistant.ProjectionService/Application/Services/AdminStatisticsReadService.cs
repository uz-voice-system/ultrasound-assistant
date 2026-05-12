using UltrasoundAssistant.Contracts.Statistics;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Application.Services;

public sealed class AdminStatisticsReadService : IAdminStatisticsReadService
{
    private readonly IAdminStatisticsReadRepository _repository;

    public AdminStatisticsReadService(IAdminStatisticsReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminStatisticsDto> GetAsync(AdminStatisticsRequest request, CancellationToken cancellationToken)
    {
        Validate(request);

        return await _repository.GetAsync(request, cancellationToken);
    }

    private static void Validate(AdminStatisticsRequest request)
    {
        if (request.DateFromUtc == default)
            throw new ArgumentException("DateFromUtc is required");

        if (request.DateToUtc == default)
            throw new ArgumentException("DateToUtc is required");

        if (request.DateFromUtc > request.DateToUtc)
            throw new ArgumentException("DateFromUtc cannot be greater than DateToUtc");
    }
}
