namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

/// <summary>
/// Найденное ключевое слово в тексте
/// </summary>
public sealed class KeywordOccurrence
{
    /// <summary>
    /// Тип найденного ключевого слова
    /// </summary>
    public KeywordMatchKind Kind { get; init; }

    /// <summary>
    /// Индекс первого слова
    /// </summary>
    public int WordIndex { get; init; }

    /// <summary>
    /// Количество слов
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// Идентификатор блока
    /// </summary>
    public Guid BlockId { get; init; }

    /// <summary>
    /// Название блока
    /// </summary>
    public string BlockName { get; init; } = string.Empty;

    /// <summary>
    /// Техническое имя поля
    /// </summary>
    public string? FieldName { get; init; }

    /// <summary>
    /// Эталонное ключевое слово
    /// </summary>
    public string Keyword { get; init; } = string.Empty;

    /// <summary>
    /// Распознанное ключевое слово
    /// </summary>
    public string RecognizedKeyword { get; init; } = string.Empty;

    /// <summary>
    /// Уверенность сопоставления
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Индекс первого слова после ключевого слова
    /// </summary>
    public int EndWordIndex => WordIndex + WordCount;
}
