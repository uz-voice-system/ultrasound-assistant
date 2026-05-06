namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Commands;

/// <summary>
/// Результат удаления фрагментов паузы из текста.
/// </summary>
public sealed class VoicePauseProcessingResult
{
    /// <summary>
    /// Текст без фрагментов между "пауза" и "продолжить".
    /// </summary>
    public string ProcessedText { get; set; } = string.Empty;

    /// <summary>
    /// Текст, который был проигнорирован из-за паузы.
    /// </summary>
    public string IgnoredText { get; set; } = string.Empty;

    /// <summary>
    /// Была ли найдена команда паузы.
    /// </summary>
    public bool PauseFound { get; set; }

    /// <summary>
    /// Была ли найдена команда продолжения.
    /// </summary>
    public bool ResumeFound { get; set; }

    /// <summary>
    /// Остался ли текст в состоянии паузы к концу распознавания.
    /// </summary>
    public bool IsPausedAtEnd { get; set; }
}
