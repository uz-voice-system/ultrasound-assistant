namespace UltrasoundAssistant.VoiceProcessingService.Options;

public sealed class WhisperOptions
{
    /// <summary>
    /// tiny, base, small, medium, large-v3, ...
    /// </summary>
    public string Model { get; set; } = "base";

    /// <summary>
    /// где хранить модель
    /// </summary>
    public string ModelsPath { get; set; } = "/app/models";

    /// <summary>
    /// если true - скачает при старте, если файла нет
    /// </summary>
    public bool DownloadOnStartup { get; set; } = true;
}
