namespace UltrasoundAssistant.Contracts.Reads.Templates.Admin;

/// <summary>
/// Совпадение при поиске шаблона
/// </summary>
public sealed class TemplateSearchMatchDto
{
    /// <summary>
    /// Тип совпадения
    /// </summary>
    public TemplateSearchMatchType Type { get; set; }

    /// <summary>
    /// Найденное значение
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор блока
    /// </summary>
    public Guid? BlockId { get; set; }

    /// <summary>
    /// Название блока
    /// </summary>
    public string? BlockName { get; set; }

    /// <summary>
    /// Идентификатор поля
    /// </summary>
    public Guid? FieldId { get; set; }

    /// <summary>
    /// Техническое имя поля
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Отображаемое название поля
    /// </summary>
    public string? FieldDisplayName { get; set; }
}
