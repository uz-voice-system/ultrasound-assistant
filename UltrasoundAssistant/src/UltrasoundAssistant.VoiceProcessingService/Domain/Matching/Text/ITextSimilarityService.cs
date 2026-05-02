namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Text;

/// <summary>
/// Сервис расчёта похожести текста
/// </summary>
public interface ITextSimilarityService
{
    /// <summary>
    /// Рассчитывает похожесть двух строк
    /// </summary>
    /// <param name="source">Первая строка</param>
    /// <param name="target">Вторая строка</param>
    /// <returns>Похожесть от 0 до 1</returns>
    double Calculate(string source, string target);

    /// <summary>
    /// Рассчитывает похожесть двух наборов слов
    /// </summary>
    /// <param name="sourceWords">Первый набор слов</param>
    /// <param name="targetWords">Второй набор слов</param>
    /// <returns>Похожесть от 0 до 1</returns>
    double CalculateWords(IReadOnlyList<string> sourceWords, IReadOnlyList<string> targetWords);
}
