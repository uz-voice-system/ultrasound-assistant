using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Values;

/// <summary>
/// Сервис извлечения значения поля из текста
/// </summary>
public interface IValueExtractor
{
    /// <summary>
    /// Извлекает значение между двумя позициями текста
    /// </summary>
    /// <param name="tokens">Токены текста</param>
    /// <param name="startIndex">Начальная позиция</param>
    /// <param name="endIndex">Конечная позиция</param>
    /// <param name="maxWords">Максимальное количество слов</param>
    /// <returns>Извлечённое значение</returns>
    string Extract(IReadOnlyList<TextToken> tokens, int startIndex, int endIndex, int maxWords);
}
