using UltrasoundAssistant.Contracts.Entity.Templates;

namespace UltrasoundAssistant.Contracts.Events.TemplateEvent;

/// <summary>
/// Поле шаблона в событии
/// </summary>
public sealed class TemplateFieldEventDto
{
    /// <summary>
    /// Идентификатор поля
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Техническое имя поля
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое название поля
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Порядок отображения поля
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Фразы поля
    /// </summary>
    public List<string> Phrases { get; set; } = [];

    /// <summary>
    /// Тип значения поля
    /// </summary>
    public TemplateFieldType Type { get; set; }

    /// <summary>
    /// Норма значения поля
    /// </summary>
    public FieldNormDto? Norm { get; set; }
}
