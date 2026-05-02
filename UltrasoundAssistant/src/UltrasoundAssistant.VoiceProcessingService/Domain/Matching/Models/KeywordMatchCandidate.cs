namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Models;

/// <summary>
/// Кандидат ключевого слова для сопоставления
/// </summary>
public sealed class KeywordMatchCandidate
{
    /// <summary>
    /// Тип кандидата
    /// </summary>
    public KeywordMatchKind Kind { get; init; }

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
    /// Фраза кандидата
    /// </summary>
    public string Phrase { get; init; } = string.Empty;

    /// <summary>
    /// Нормализованные слова фразы
    /// </summary>
    public IReadOnlyList<string> NormalizedWords { get; init; } = [];
}
