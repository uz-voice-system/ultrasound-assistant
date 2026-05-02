using UltrasoundAssistant.Contracts.Reads.Templates.Details;

namespace UltrasoundAssistant.VoiceProcessingService.Application.Abstractions;

/// <summary>
/// Сервис получения шаблонов отчётов по идентификатору
/// </summary>
public interface ITemplateLookupService
{
    /// <summary>
    /// Получает шаблон отчёта
    /// </summary>
    /// <param name="templateId">Идентификатор шаблона</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Шаблон отчёта или null</returns>
    Task<TemplateDto?> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken);
}
