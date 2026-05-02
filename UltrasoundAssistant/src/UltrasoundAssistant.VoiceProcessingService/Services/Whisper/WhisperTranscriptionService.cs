using System.Text;
using UltrasoundAssistant.VoiceProcessingService.Application.Abstractions;
using Whisper.net;

namespace UltrasoundAssistant.VoiceProcessingService.Services.Whisper;

public sealed class WhisperTranscriptionService : IWhisperTranscriptionService
{
    private readonly IWhisperModelManager _modelManager;
    private readonly ILogger<WhisperTranscriptionService> _logger;

    private WhisperFactory? _factory;
    private string? _loadedModelPath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public WhisperTranscriptionService(
        IWhisperModelManager modelManager,
        ILogger<WhisperTranscriptionService> logger)
    {
        _modelManager = modelManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> TranscribeAsync(
        byte[] audioBytes,
        string language,
        CancellationToken cancellationToken)
    {
        if (audioBytes is null || audioBytes.Length == 0)
            return string.Empty;

        var modelPath = await _modelManager.EnsureModelAsync(cancellationToken);
        await EnsureFactoryAsync(modelPath, cancellationToken);

        await using var audioStream = new MemoryStream(audioBytes, writable: false);

        using var processor = _factory!
            .CreateBuilder()
            .WithLanguage(string.IsNullOrWhiteSpace(language) ? "ru" : language)
            .Build();

        var sb = new StringBuilder();

        await foreach (var segment in processor.ProcessAsync(audioStream))
        {
            if (!string.IsNullOrWhiteSpace(segment.Text))
            {
                if (sb.Length > 0)
                    sb.Append(' ');

                sb.Append(segment.Text.Trim());
            }
        }

        var text = sb.ToString().Trim();
        _logger.LogInformation("Whisper recognized text: {Text}", text);

        return text;
    }

    private async Task EnsureFactoryAsync(string modelPath, CancellationToken cancellationToken)
    {
        if (_factory is not null && string.Equals(_loadedModelPath, modelPath, StringComparison.OrdinalIgnoreCase))
            return;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_factory is not null && string.Equals(_loadedModelPath, modelPath, StringComparison.OrdinalIgnoreCase))
                return;

            _factory?.Dispose();

            _logger.LogInformation("Loading Whisper factory from model {ModelPath}", modelPath);
            _factory = WhisperFactory.FromPath(modelPath);
            _loadedModelPath = modelPath;
            _logger.LogInformation("Whisper factory loaded");
        }
        finally
        {
            _lock.Release();
        }
    }
}
