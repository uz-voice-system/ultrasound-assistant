

namespace UltrasoundAssistant.Contracts.Templates
{
    /// <summary>
    /// DTO ключевого слова шаблона.
    /// </summary>
    public class TemplateKeywordDto
    {
        /// <summary>
        /// Идентификатор ключевого слова (в Read DB — surrogate key).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Фраза, которую должен произнести врач.
        /// </summary>
        public string Phrase { get; set; } = null!;

        /// <summary>
        /// Целевое поле отчёта (например: interventricular_septum).
        /// </summary>
        public string TargetField { get; set; } = null!;
    }
}
