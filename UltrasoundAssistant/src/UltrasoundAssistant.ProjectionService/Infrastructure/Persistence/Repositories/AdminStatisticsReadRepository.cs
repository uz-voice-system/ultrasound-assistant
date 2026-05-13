using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.Contracts.Common;
using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Statistics;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Repositories;

public sealed class AdminStatisticsReadRepository : IAdminStatisticsReadRepository
{
    private readonly ProjectionDbContext _dbContext;

    public AdminStatisticsReadRepository(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminStatisticsDto> GetAsync(
        AdminStatisticsRequest request,
        CancellationToken cancellationToken)
    {
        var appointmentsQuery = _dbContext.Appointments
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.StartAtUtc >= request.DateFromUtc &&
                x.StartAtUtc <= request.DateToUtc);

        if (request.DoctorId is not null)
            appointmentsQuery = appointmentsQuery.Where(x => x.DoctorId == request.DoctorId.Value);

        if (request.TemplateId is not null)
            appointmentsQuery = appointmentsQuery.Where(x => x.TemplateId == request.TemplateId.Value);

        var appointments = await appointmentsQuery
            .Select(x => new AppointmentStatisticsItem
            {
                Id = x.Id,
                PatientId = x.PatientId,
                DoctorId = x.DoctorId,
                TemplateId = x.TemplateId,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);

        var appointmentIds = appointments
            .Select(x => x.Id)
            .ToHashSet();

        var reports = await _dbContext.Reports
            .AsNoTracking()
            .Where(x => !x.IsDeleted && appointmentIds.Contains(x.AppointmentId))
            .Select(x => new ReportStatisticsItem
            {
                Id = x.Id,
                AppointmentId = x.AppointmentId,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);

        var doctorIds = appointments
            .Select(x => x.DoctorId)
            .Distinct()
            .ToList();

        var templateIds = appointments
            .Select(x => x.TemplateId)
            .Distinct()
            .ToList();

        var doctors = await _dbContext.Users
            .AsNoTracking()
            .Where(x => doctorIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.FullName
            })
            .ToDictionaryAsync(
                x => x.Id,
                x => x.FullName,
                cancellationToken);

        var templates = await _dbContext.Templates
            .AsNoTracking()
            .Where(x => templateIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.Name
            })
            .ToDictionaryAsync(
                x => x.Id,
                x => x.Name,
                cancellationToken);

        var reportAppointmentIds = reports
            .Select(x => x.AppointmentId)
            .ToHashSet();

        var acceptedAppointments = appointments
            .Where(x => IsAcceptedStatus(x.Status))
            .ToList();

        var appointmentsWithoutReport = acceptedAppointments
            .Where(x => !reportAppointmentIds.Contains(x.Id))
            .ToList();

        return new AdminStatisticsDto
        {
            DateFromUtc = request.DateFromUtc,
            DateToUtc = request.DateToUtc,

            TotalAppointmentsCount = appointments.Count,
            AcceptedAppointmentsCount = acceptedAppointments.Count,

            UniqueAcceptedPatientsCount = acceptedAppointments
                .Select(x => x.PatientId)
                .Distinct()
                .Count(),

            ReportsCount = reports.Count,
            AppointmentsWithoutReportCount = appointmentsWithoutReport.Count,

            ScheduledAppointmentsCount = appointments.Count(x => x.Status == AppointmentStatus.Scheduled),
            InProgressAppointmentsCount = appointments.Count(x => x.Status == AppointmentStatus.InProgress),
            CompletedAppointmentsCount = appointments.Count(x => x.Status == AppointmentStatus.Completed),
            NoShowAppointmentsCount = appointments.Count(x => x.Status == AppointmentStatus.NoShow),

            Doctors = BuildDoctorStatistics(
                appointments,
                reports,
                doctors),

            Templates = BuildTemplateStatistics(
                appointments,
                reports,
                templates),

            AppointmentStatuses = appointments
                .GroupBy(x => x.Status)
                .Select(x => new AppointmentStatusStatisticsDto
                {
                    Status = x.Key.ToString(),
                    StatusDisplayName = x.Key.GetDisplayName(),
                    Count = x.Count()
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.StatusDisplayName)
                .ToList(),

            ReportStatuses = reports
                .GroupBy(x => x.Status)
                .Select(x => new ReportStatusStatisticsDto
                {
                    Status = x.Key.ToString(),
                    StatusDisplayName = x.Key.GetDisplayName(),
                    Count = x.Count()
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.StatusDisplayName)
                .ToList()
        };
    }

    private static List<DoctorStatisticsDto> BuildDoctorStatistics(
        IReadOnlyList<AppointmentStatisticsItem> appointments,
        IReadOnlyList<ReportStatisticsItem> reports,
        IReadOnlyDictionary<Guid, string> doctors)
    {
        var reportsByAppointmentId = reports
            .GroupBy(x => x.AppointmentId)
            .ToDictionary(x => x.Key, x => x.Count());

        return appointments
            .GroupBy(x => x.DoctorId)
            .Select(group =>
            {
                var groupAppointments = group.ToList();

                var acceptedAppointments = groupAppointments
                    .Where(x => IsAcceptedStatus(x.Status))
                    .ToList();

                return new DoctorStatisticsDto
                {
                    DoctorId = group.Key,

                    DoctorFullName = doctors.TryGetValue(group.Key, out var fullName)
                        ? fullName
                        : "Неизвестный врач",

                    AppointmentsCount = groupAppointments.Count,
                    AcceptedAppointmentsCount = acceptedAppointments.Count,

                    InProgressAppointmentsCount = groupAppointments.Count(x =>
                        x.Status == AppointmentStatus.InProgress),

                    CompletedAppointmentsCount = groupAppointments.Count(x =>
                        x.Status == AppointmentStatus.Completed),

                    NoShowAppointmentsCount = groupAppointments.Count(x =>
                        x.Status == AppointmentStatus.NoShow),

                    UniqueAcceptedPatientsCount = acceptedAppointments
                        .Select(x => x.PatientId)
                        .Distinct()
                        .Count(),

                    ReportsCount = groupAppointments.Sum(x =>
                        reportsByAppointmentId.TryGetValue(x.Id, out var count)
                            ? count
                            : 0)
                };
            })
            .OrderByDescending(x => x.AcceptedAppointmentsCount)
            .ThenBy(x => x.DoctorFullName)
            .ToList();
    }

    private static List<TemplateStatisticsDto> BuildTemplateStatistics(
        IReadOnlyList<AppointmentStatisticsItem> appointments,
        IReadOnlyList<ReportStatisticsItem> reports,
        IReadOnlyDictionary<Guid, string> templates)
    {
        var reportsByAppointmentId = reports
            .GroupBy(x => x.AppointmentId)
            .ToDictionary(x => x.Key, x => x.Count());

        return appointments
            .GroupBy(x => x.TemplateId)
            .Select(group =>
            {
                var groupAppointments = group.ToList();

                var acceptedAppointments = groupAppointments
                    .Where(x => IsAcceptedStatus(x.Status))
                    .ToList();

                return new TemplateStatisticsDto
                {
                    TemplateId = group.Key,

                    TemplateName = templates.TryGetValue(group.Key, out var name)
                        ? name
                        : "Неизвестный шаблон",

                    AppointmentsCount = groupAppointments.Count,
                    AcceptedAppointmentsCount = acceptedAppointments.Count,

                    InProgressAppointmentsCount = groupAppointments.Count(x =>
                        x.Status == AppointmentStatus.InProgress),

                    CompletedAppointmentsCount = groupAppointments.Count(x =>
                        x.Status == AppointmentStatus.Completed),

                    NoShowAppointmentsCount = groupAppointments.Count(x =>
                        x.Status == AppointmentStatus.NoShow),

                    UniqueAcceptedPatientsCount = acceptedAppointments
                        .Select(x => x.PatientId)
                        .Distinct()
                        .Count(),

                    ReportsCount = groupAppointments.Sum(x =>
                        reportsByAppointmentId.TryGetValue(x.Id, out var count)
                            ? count
                            : 0)
                };
            })
            .OrderByDescending(x => x.AcceptedAppointmentsCount)
            .ThenBy(x => x.TemplateName)
            .ToList();
    }

    private static bool IsAcceptedStatus(AppointmentStatus status)
    {
        return status is AppointmentStatus.InProgress or AppointmentStatus.Completed;
    }

    private sealed class AppointmentStatisticsItem
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public Guid DoctorId { get; set; }

        public Guid TemplateId { get; set; }

        public AppointmentStatus Status { get; set; }
    }

    private sealed class ReportStatisticsItem
    {
        public Guid Id { get; set; }

        public Guid AppointmentId { get; set; }

        public ReportStatus Status { get; set; }
    }
}
