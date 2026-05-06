namespace UltrasoundAssistant.ApiGateway.Options;

/// <summary>
/// Настройки аудита
/// </summary>
public sealed class AuditOptions
{
    /// <summary>
    /// Включён ли аудит
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Сохранять ли тело запроса
    /// </summary>
    public bool CaptureRequestBody { get; set; } = true;

    /// <summary>
    /// Максимальный размер тела запроса для аудита
    /// </summary>
    public int MaxRequestBodyLength { get; set; } = 20_000;

    /// <summary>
    /// Пути, которые не надо писать в аудит
    /// </summary>
    public List<string> ExcludedPathPrefixes { get; set; } =
    [
        "/swagger",
        "/health"
    ];
}
