using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Keywords;

/// <summary>
/// Сервис поиска ключевых слов в распознанном тексте
/// </summary>
public interface IKeywordMatcher
{
    /// <summary>
    /// Ищет блок в указанной позиции
    /// </summary>
    /// <param name="tokens">Токены текста</param>
    /// <param name="template">Шаблон отчёта</param>
    /// <param name="startIndex">Начальная позиция</param>
    /// <returns>Найденный блок или null</returns>
    KeywordOccurrence? FindBestBlockMatch(IReadOnlyList<TextToken> tokens, TemplateDto template, int startIndex);

    /// <summary>
    /// Ищет поле блока в указанной позиции
    /// </summary>
    /// <param name="tokens">Токены текста</param>
    /// <param name="block">Блок шаблона</param>
    /// <param name="startIndex">Начальная позиция</param>
    /// <returns>Найденное поле или null</returns>
    KeywordOccurrence? FindBestFieldMatch(IReadOnlyList<TextToken> tokens, TemplateBlockDto block, int startIndex);
}
