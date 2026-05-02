using UltrasoundAssistant.Contracts.VoiceProcessing;
using UltrasoundAssistant.VoiceProcessingService.Domain;

namespace UltrasoundAssistant.VoiceProcessingService.Application.Abstractions;

/// <summary>
/// Сценарий обработки голосового ввода
/// </summary>
public interface IVoiceProcessingUseCase
{
    /// <summary>
    /// Обрабатывает запрос голосового ввода
    /// </summary>
    /// <param name="request">Запрос на обработку</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат выполнения сценария</returns>
    Task<VoiceProcessingUseCaseResult> ProcessAsync(VoiceProcessRequest request, CancellationToken cancellationToken);
}
