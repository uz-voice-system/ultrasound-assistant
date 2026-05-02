using System.Text.Json;

namespace UltrasoundAssistant.ApiGateway.Services;

public sealed class VoiceProcessingApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public VoiceProcessingApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(int StatusCode, string Content)> PostAsync<T>(
        string path, T body, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(path, body, JsonOptions, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return ((int)response.StatusCode, content);
    }
}
