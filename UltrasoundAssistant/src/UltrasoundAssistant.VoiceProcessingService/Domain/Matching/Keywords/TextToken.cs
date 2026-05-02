namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

/// <summary>
/// Токен распознанного текста
/// </summary>
public sealed class TextToken
{
    /// <summary>
    /// Позиция токена в тексте
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Исходное значение токена
    /// </summary>
    public string Original { get; init; } = string.Empty;

    /// <summary>
    /// Нормализованное значение токена
    /// </summary>
    public string Normalized { get; init; } = string.Empty;
}
