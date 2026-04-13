namespace UltrasoundAssistant.Contracts.Commands.Reports;

public sealed class ProcessVoiceDataCommand
{
    public Guid CommandId { get; set; }
    public Guid ReportId { get; set; }
    public int ExpectedVersion { get; set; }

    public string RecognizedText { get; set; } = string.Empty;

    // Можно передать уже найденное ключевое слово, если клиент когда-нибудь будет уметь это делать
    public string? DetectedKeyword { get; set; }

    // Можно передать уже выделенное значение
    public string? DetectedValue { get; set; }

    public double Confidence { get; set; } = 1.0;
}