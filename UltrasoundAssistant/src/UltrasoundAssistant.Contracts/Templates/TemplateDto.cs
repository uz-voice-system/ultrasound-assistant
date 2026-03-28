

namespace UltrasoundAssistant.Contracts.Templates
{
    /// <summary>
    /// DTO шаблона отчёта.
    /// </summary>
    public class TemplateDto
    {
        /// <summary>
        /// Идентификатор шаблона.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Название шаблона.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Ключевые слова шаблона.
        /// </summary>
        public List<TemplateKeywordDto> Keywords { get; set; } = [];
    }
}
