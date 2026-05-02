namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

/// <summary>
/// Результат разбора числового значения
/// </summary>
public sealed class NumberParseResult
{
    /// <summary>
    /// Признак успешного разбора
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Извлечённое число
    /// </summary>
    public decimal Value { get; init; }

    /// <summary>
    /// Количество использованных токенов
    /// </summary>
    public int UsedTokens { get; init; }

    /// <summary>
    /// Уверенность разбора
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Возвращает неуспешный результат
    /// </summary>
    /// <returns>Неуспешный результат разбора</returns>
    public static NumberParseResult Failed()
    {
        return new NumberParseResult
        {
            Success = false,
            Value = 0,
            UsedTokens = 0,
            Confidence = 0
        };
    }
}
