namespace UltrasoundAssistant.VoiceProcessingService.Application.Abstractions;

/// <summary>
/// Менеджер модели Whisper
/// </summary>
public interface IWhisperModelManager
{
    /// <summary>
    /// Обеспечивает наличие модели
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Путь к модели</returns>
    Task<string> EnsureModelAsync(CancellationToken cancellationToken);
}
