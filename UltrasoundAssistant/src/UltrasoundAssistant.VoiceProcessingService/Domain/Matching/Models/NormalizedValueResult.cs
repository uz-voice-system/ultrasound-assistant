using UltrasoundAssistant.Contracts.VoiceProcessing;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

/// <summary>
/// Результат нормализации значения
/// </summary>
public sealed class NormalizedValueResult
{
    /// <summary>
    /// Нормализованное значение
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Числовое значение
    /// </summary>
    public decimal? NumericValue { get; init; }

    /// <summary>
    /// Единица измерения
    /// </summary>
    public string? Unit { get; init; }

    /// <summary>
    /// Уверенность нормализации
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Статус проверки по норме
    /// </summary>
    public NormStatus NormStatus { get; init; } = NormStatus.Unknown;

    /// <summary>
    /// Сообщение по результату проверки нормы
    /// </summary>
    public string? NormMessage { get; init; }

    /// <summary>
    /// Возвращает неуспешный результат
    /// </summary>
    /// <returns>Неуспешный результат нормализации</returns>
    public static NormalizedValueResult Failed()
    {
        return new NormalizedValueResult
        {
            Value = string.Empty,
            NumericValue = null,
            Unit = null,
            Confidence = 0,
            NormStatus = NormStatus.Unknown
        };
    }
}
