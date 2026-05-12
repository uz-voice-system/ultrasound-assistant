namespace UltrasoundAssistant.Contracts.Statistics;

/// <summary>
/// Статистика администратора.
/// </summary>
public sealed class AdminStatisticsDto
{
    /// <summary>
    /// Дата начала периода.
    /// </summary>
    public DateTime DateFromUtc { get; set; }

    /// <summary>
    /// Дата окончания периода.
    /// </summary>
    public DateTime DateToUtc { get; set; }

    /// <summary>
    /// Общее количество записей на приём.
    /// </summary>
    public int TotalAppointmentsCount { get; set; }

    /// <summary>
    /// Количество принятых пациентов.
    /// Принятой считается запись, по которой есть отчёт.
    /// </summary>
    public int AcceptedAppointmentsCount { get; set; }

    /// <summary>
    /// Количество уникальных принятых пациентов.
    /// </summary>
    public int UniqueAcceptedPatientsCount { get; set; }

    /// <summary>
    /// Количество созданных отчётов.
    /// </summary>
    public int ReportsCount { get; set; }

    /// <summary>
    /// Количество записей без отчёта.
    /// </summary>
    public int AppointmentsWithoutReportCount { get; set; }

    /// <summary>
    /// Статистика по врачам.
    /// </summary>
    public List<DoctorStatisticsDto> Doctors { get; set; } = [];

    /// <summary>
    /// Статистика по шаблонам исследований.
    /// </summary>
    public List<TemplateStatisticsDto> Templates { get; set; } = [];

    /// <summary>
    /// Статистика по статусам отчётов.
    /// </summary>
    public List<ReportStatusStatisticsDto> ReportStatuses { get; set; } = [];
}
