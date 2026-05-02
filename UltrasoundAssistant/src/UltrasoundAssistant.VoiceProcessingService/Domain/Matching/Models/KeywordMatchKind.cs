namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

/// <summary>
/// Тип найденного ключевого слова
/// </summary>
public enum KeywordMatchKind
{
    /// <summary>
    /// Блок шаблона
    /// </summary>
    Block = 0,

    /// <summary>
    /// Поле блока
    /// </summary>
    Field = 1
}
