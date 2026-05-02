namespace UltrasoundAssistant.Contracts.Reads.Templates.Search;

/// <summary>
/// Краткая информация о шаблоне
/// </summary>
public sealed class TemplateSummaryDto
{
    /// <summary>
    /// Идентификатор шаблона
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Название шаблона
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Признак удаления
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Версия шаблона
    /// </summary>
    public int Version { get; set; }
}
