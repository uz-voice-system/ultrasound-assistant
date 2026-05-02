namespace UltrasoundAssistant.Contracts.Reads.Templates.Admin;

/// <summary>
/// Тип совпадения при поиске шаблона
/// </summary>
public enum TemplateSearchMatchType
{
    /// <summary>
    /// Название шаблона
    /// </summary>
    TemplateName = 0,

    /// <summary>
    /// Название блока
    /// </summary>
    BlockName = 1,

    /// <summary>
    /// Фраза блока
    /// </summary>
    BlockPhrase = 2,

    /// <summary>
    /// Техническое имя поля
    /// </summary>
    FieldName = 3,

    /// <summary>
    /// Отображаемое название поля
    /// </summary>
    FieldDisplayName = 4,

    /// <summary>
    /// Фраза поля
    /// </summary>
    FieldPhrase = 5,

    /// <summary>
    /// Тип поля
    /// </summary>
    FieldType = 6,

    /// <summary>
    /// Норма поля
    /// </summary>
    FieldNorm = 7
}
