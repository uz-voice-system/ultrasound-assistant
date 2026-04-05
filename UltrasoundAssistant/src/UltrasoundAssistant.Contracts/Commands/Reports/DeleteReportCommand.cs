namespace UltrasoundAssistant.Contracts.Commands.Reports;

public sealed class DeleteReportCommand
{
    public Guid CommandId { get; set; }
    public Guid ReportId { get; set; }
    public int ExpectedVersion { get; set; }
}
