using Microsoft.Extensions.Options;
using UltrasoundAssistant.VoiceProcessingService.Application.Abstractions;
using UltrasoundAssistant.VoiceProcessingService.Options;
using Whisper.net.Ggml;

namespace UltrasoundAssistant.VoiceProcessingService.Services.Whisper;

public sealed class WhisperModelManager : IWhisperModelManager
{
    private readonly WhisperOptions _options;
    private readonly ILogger<WhisperModelManager> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _cachedPath;

    public WhisperModelManager(
        IOptions<WhisperOptions> options,
        ILogger<WhisperModelManager> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> EnsureModelAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_cachedPath) && File.Exists(_cachedPath))
            return _cachedPath;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(_cachedPath) && File.Exists(_cachedPath))
                return _cachedPath;

            var modelsPath = Path.GetFullPath(_options.ModelsPath);
            Directory.CreateDirectory(modelsPath);

            var (ggmlType, fileName) = ResolveModel(_options.Model);
            var fullPath = Path.Combine(modelsPath, fileName);

            if (File.Exists(fullPath))
            {
                var info = new FileInfo(fullPath);
                if (info.Length > 0)
                {
                    _logger.LogInformation("Whisper model already exists: {Path}", fullPath);
                    _cachedPath = fullPath;
                    return fullPath;
                }
            }

            _logger.LogInformation("Whisper model {Model} not found. Downloading to {Path}", _options.Model, fullPath);

            var tempPath = fullPath + ".tmp";

            using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(ggmlType);
            await using (var fileStream = File.Create(tempPath))
            {
                await modelStream.CopyToAsync(fileStream, cancellationToken);
                await fileStream.FlushAsync(cancellationToken);
            }

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            File.Move(tempPath, fullPath);

            _logger.LogInformation("Whisper model downloaded: {Path}", fullPath);

            _cachedPath = fullPath;
            return fullPath;
        }
        finally
        {
            _lock.Release();
        }
    }

    private static (GgmlType Type, string FileName) ResolveModel(string model)
    {
        return model.ToLowerInvariant() switch
        {
            "tiny" => (GgmlType.Tiny, "ggml-tiny.bin"),
            "tiny.en" => (GgmlType.TinyEn, "ggml-tiny.en.bin"),
            "base" => (GgmlType.Base, "ggml-base.bin"),
            "base.en" => (GgmlType.BaseEn, "ggml-base.en.bin"),
            "small" => (GgmlType.Small, "ggml-small.bin"),
            "small.en" => (GgmlType.SmallEn, "ggml-small.en.bin"),
            "medium" => (GgmlType.Medium, "ggml-medium.bin"),
            "medium.en" => (GgmlType.MediumEn, "ggml-medium.en.bin"),
            "largev3turbo" => (GgmlType.LargeV3Turbo, "ggml-large-v3-turbo.bin"),
            "large-v3-turbo" => (GgmlType.LargeV3Turbo, "ggml-large-v3-turbo.bin"),
            _ => throw new InvalidOperationException($"Unsupported Whisper model: {model}")
        };
    }
}
