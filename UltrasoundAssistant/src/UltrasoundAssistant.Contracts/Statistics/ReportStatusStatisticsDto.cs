namespace UltrasoundAssistant.Contracts.Statistics;

/// <summary>
/// Статистика по статусу отчёта.
/// </summary>
public sealed class ReportStatusStatisticsDto
{
    /// <summary>
    /// Статус отчёта.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Количество отчётов.
    /// </summary>
    public int Count { get; set; }
}
