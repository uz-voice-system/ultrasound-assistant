namespace UltrasoundAssistant.Contracts.Reads.Templates.Details
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
        /// Признак удаления
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Блоки шаблона
        /// </summary>
        public List<TemplateBlockDto> Blocks { get; set; } = [];

        /// <summary>
        /// Версия агрегата (для команд обновления и удаления).
        /// </summary>
        public int Version { get; set; }
    }
}
