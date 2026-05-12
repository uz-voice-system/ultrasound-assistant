namespace UltrasoundAssistant.ApiGateway.Contracts;

public sealed class UploadReportImageForm
{
    /// <summary>
    /// Изображение УЗИ.
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Ожидаемая версия отчёта.
    /// </summary>
    public int ExpectedVersion { get; set; }
}
