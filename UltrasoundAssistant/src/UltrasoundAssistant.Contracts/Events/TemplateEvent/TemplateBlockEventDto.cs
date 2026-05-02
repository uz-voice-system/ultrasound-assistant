namespace UltrasoundAssistant.Contracts.Events.TemplateEvent;

/// <summary>
/// Блок шаблона в событии
/// </summary>
public sealed class TemplateBlockEventDto
{
    /// <summary>
    /// Идентификатор блока
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Название блока
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Порядок отображения блока
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Фразы блока
    /// </summary>
    public List<string> Phrases { get; set; } = [];

    /// <summary>
    /// Поле по умолчанию
    /// </summary>
    public string? DefaultFieldName { get; set; }

    /// <summary>
    /// Поля блока
    /// </summary>
    public List<TemplateFieldEventDto> Fields { get; set; } = [];
}
