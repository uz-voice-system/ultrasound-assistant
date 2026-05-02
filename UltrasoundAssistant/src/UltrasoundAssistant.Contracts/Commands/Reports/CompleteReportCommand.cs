namespace UltrasoundAssistant.Contracts.Commands.Reports;

public sealed class CompleteReportCommand
{
    public Guid ReportId { get; set; }
    public int ExpectedVersion { get; set; }
}
