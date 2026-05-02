namespace UltrasoundAssistant.Contracts.Reads.Templates.Search;

/// <summary>
/// Фильтр поиска шаблонов для выбора
/// </summary>
public sealed class TemplateSearchRequest
{
    /// <summary>
    /// Строка поиска по названию шаблона
    /// </summary>
    public string? SearchText { get; set; }
}
