namespace UltrasoundAssistant.VoiceProcessingService.Application.Abstractions;

/// <summary>
/// Сервис распознавания речи
/// </summary>
public interface IWhisperTranscriptionService
{
    /// <summary>
    /// Выполняет распознавание речи
    /// </summary>
    /// <param name="audioBytes">Аудиоданные</param>
    /// <param name="language">Язык распознавания</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Распознанный текст</returns>
    Task<string> TranscribeAsync(byte[] audioBytes, string language, CancellationToken cancellationToken);
}
