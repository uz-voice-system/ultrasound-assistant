using System.Text.Json;
using System.Text.Json.Serialization;

namespace UltrasoundAssistant.ApiGateway.Services;

public sealed class ProjectionApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public ProjectionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(int StatusCode, string Content)> GetAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(path, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return ((int)response.StatusCode, content);
    }

    public async Task<(int StatusCode, string Content)> PostAsync<T>(
        string path,
        T body,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(path, body, JsonOptions, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return ((int)response.StatusCode, content);
    }
}