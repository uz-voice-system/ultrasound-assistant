using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.ReportEvent;

/// <summary>
/// Событие загрузки изображения УЗИ для отчёта.
/// </summary>
public sealed class ReportImageUploadedEvent : IEvent
{
    /// <summary>
    /// Идентификатор события.
    /// </summary>
    public Guid EventId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Дата создания события.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Идентификатор отчёта.
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Название файла.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME-тип изображения.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Изображение в формате Base64.
    /// </summary>
    public string ImageBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Дата загрузки изображения.
    /// </summary>
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Версия агрегата.
    /// </summary>
    public int Version { get; set; }
}
