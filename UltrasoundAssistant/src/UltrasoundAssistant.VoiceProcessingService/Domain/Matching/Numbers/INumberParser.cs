using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Numbers;

/// <summary>
/// Парсер числовых значений из текста
/// </summary>
public interface INumberParser
{
    /// <summary>
    /// Пытается извлечь число из токенов
    /// </summary>
    /// <param name="tokens">Токены значения</param>
    /// <returns>Результат разбора числа</returns>
    NumberParseResult Parse(IReadOnlyList<string> tokens);
}
