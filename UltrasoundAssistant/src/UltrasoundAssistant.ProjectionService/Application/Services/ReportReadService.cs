using UltrasoundAssistant.Contracts.Reads.Reports.Details;
using UltrasoundAssistant.Contracts.Reads.Reports.Search;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Mapping;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Application.Services;

public sealed class ReportReadService : IReportReadService
{
    private readonly IReportReadRepository _repository;
    private readonly ReportProjectionMapper _mapper;

    public ReportReadService(IReportReadRepository repository, ReportProjectionMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ReportDto?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var report = await _repository.GetByIdAsync(id, includeDeleted, cancellationToken);

        return report is null ? null : _mapper.MapFull(report);
    }

    public async Task<ReportDto?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        var report = await _repository.GetByAppointmentIdAsync(appointmentId, cancellationToken);

        return report is null ? null : _mapper.MapFull(report);
    }

    public async Task<IReadOnlyList<ReportSummaryDto>> SearchAsync(ReportSearchRequest filter, CancellationToken cancellationToken)
    {
        var reports = await _repository.SearchAsync(filter, cancellationToken);

        return reports.Select(_mapper.MapSummary).ToList();
    }
}