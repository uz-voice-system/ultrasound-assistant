using UltrasoundAssistant.Contracts.Entity.Templates;

namespace UltrasoundAssistant.Contracts.Commands.Templates;

/// <summary>
/// Команда создания шаблона
/// </summary>
public sealed class CreateTemplateCommand
{
    /// <summary>
    /// Идентификатор шаблона
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Название шаблона
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Длительность приёма по умолчанию в минутах
    /// </summary>
    public int DefaultAppointmentDurationMinutes { get; set; }

    /// <summary>
    /// Блоки шаблона
    /// </summary>
    public List<TemplateBlockDto> Blocks { get; set; } = [];
}
