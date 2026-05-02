using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.Contracts.VoiceProcessing;

namespace UltrasoundAssistant.VoiceProcessingService.Application.Abstractions;

/// <summary>
/// Сервис сопоставления текста с ключевыми словами шаблона
/// </summary>
public interface ITemplateMatchingService
{
    /// <summary>
    /// Выполняет сопоставление текста с ключевыми словами
    /// </summary>
    /// <param name="recognizedText">Распознанный текст</param>
    /// <param name="template">Шаблон отчета</param>
    /// <returns>Результат сопоставления</returns>
    VoiceProcessResult Match(string recognizedText, TemplateDto template);
}
