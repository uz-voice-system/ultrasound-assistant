namespace UltrasoundAssistant.ApiGateway.Infrastructure.Persistence.Entities;

/// <summary>
/// Запись аудита HTTP-запроса
/// </summary>
public sealed class AuditLogEntity
{
    /// <summary>
    /// Идентификатор записи аудита
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор трассировки запроса
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Логин пользователя
    /// </summary>
    public string? UserLogin { get; set; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public string? UserRole { get; set; }

    /// <summary>
    /// HTTP-метод
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Путь запроса
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Query string
    /// </summary>
    public string? QueryString { get; set; }

    /// <summary>
    /// Endpoint ASP.NET Core
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Операция
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Тип сущности
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Идентификатор сущности
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// HTTP-статус ответа
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Признак успешного выполнения
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Текст ошибки
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Дата начала запроса
    /// </summary>
    public DateTime StartedAtUtc { get; set; }

    /// <summary>
    /// Дата окончания запроса
    /// </summary>
    public DateTime FinishedAtUtc { get; set; }

    /// <summary>
    /// Длительность выполнения в миллисекундах
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// IP-адрес клиента
    /// </summary>
    public string? ClientIp { get; set; }

    /// <summary>
    /// User-Agent клиента
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Тело запроса после очистки чувствительных данных
    /// </summary>
    public string? RequestBodyJson { get; set; }
}
