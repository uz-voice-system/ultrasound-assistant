namespace UltrasoundAssistant.Contracts.Reports;

public sealed class DeleteReportRequest
{
    public Guid CommandId { get; set; }
    public int ExpectedVersion { get; set; }
}
