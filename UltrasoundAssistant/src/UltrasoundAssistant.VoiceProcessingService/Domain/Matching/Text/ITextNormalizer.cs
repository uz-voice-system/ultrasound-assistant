using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Text;

/// <summary>
/// Нормализатор распознанного текста
/// </summary>
public interface ITextNormalizer
{
    /// <summary>
    /// Нормализует текст
    /// </summary>
    /// <param name="text">Исходный текст</param>
    /// <returns>Нормализованный текст</returns>
    string NormalizeText(string text);

    /// <summary>
    /// Нормализует слово
    /// </summary>
    /// <param name="word">Исходное слово</param>
    /// <returns>Нормализованное слово</returns>
    string NormalizeWord(string word);

    /// <summary>
    /// Разбивает текст на нормализованные токены
    /// </summary>
    /// <param name="text">Исходный текст</param>
    /// <returns>Список токенов</returns>
    IReadOnlyList<TextToken> Tokenize(string text);
}
