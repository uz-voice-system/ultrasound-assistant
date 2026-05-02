using System.Text.Json;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.VoiceProcessingService.Application.Abstractions;

namespace UltrasoundAssistant.VoiceProcessingService.Services.Templates;

public sealed class TemplateLookupService : ITemplateLookupService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public TemplateLookupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<TemplateDto?> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"/api/read/templates/{templateId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<TemplateDto>(json, JsonOptions);
    }
}
