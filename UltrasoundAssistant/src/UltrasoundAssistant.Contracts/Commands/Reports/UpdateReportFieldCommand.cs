namespace UltrasoundAssistant.Contracts.Commands.Reports;

public sealed class UpdateReportFieldCommand
{
    public Guid ReportId { get; set; }
    public int ExpectedVersion { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; } = 1.0;
}
