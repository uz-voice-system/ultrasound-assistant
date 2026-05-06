namespace UltrasoundAssistant.ApiGateway.Options;

/// <summary>
/// Настройки JWT-аутентификации
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Издатель токена
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Получатель токена
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Секретный ключ
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Время жизни токена в минутах
    /// </summary>
    public int ExpirationMinutes { get; set; } = 720;
}
