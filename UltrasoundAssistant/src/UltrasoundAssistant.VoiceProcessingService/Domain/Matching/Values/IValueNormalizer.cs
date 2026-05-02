using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Values;

/// <summary>
/// Нормализатор значений полей отчёта
/// </summary>
public interface IValueNormalizer
{
    /// <summary>
    /// Нормализует значение поля
    /// </summary>
    /// <param name="rawValue">Исходное значение</param>
    /// <param name="field">Поле шаблона</param>
    /// <returns>Результат нормализации</returns>
    NormalizedValueResult Normalize(string rawValue, TemplateFieldDto field);
}
