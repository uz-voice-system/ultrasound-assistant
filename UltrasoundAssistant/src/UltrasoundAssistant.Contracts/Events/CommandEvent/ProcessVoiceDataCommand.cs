

namespace UltrasoundAssistant.Contracts.Events.CommandEvent
{
    /// <summary>
    /// Команда обработки голосовых данных.
    /// Используется для передачи результата распознавания речи.
    /// </summary>
    public class ProcessVoiceDataCommand
    {
        /// <summary>
        /// Идентификатор отчёта.
        /// </summary>
        public Guid ReportId { get; set; }

        /// <summary>
        /// Распознанный текст.
        /// </summary>
        public string RecognizedText { get; set; } = null!;

        /// <summary>
        /// Уровень уверенности распознавания (0-1).
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Обнаруженное ключевое слово.
        /// </summary>
        public string? DetectedKeyword { get; set; }

        /// <summary>
        /// Значение, извлечённое из речи.
        /// </summary>
        public string? DetectedValue { get; set; }
    }
}
