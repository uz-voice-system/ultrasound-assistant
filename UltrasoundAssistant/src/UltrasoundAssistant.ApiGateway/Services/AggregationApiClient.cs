using System.Text;
using System.Text.Json;

namespace UltrasoundAssistant.ApiGateway.Services;

public sealed class AggregationApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public AggregationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

    public async Task<(int StatusCode, string Content)> PutAsync<T>(
        string path,
        T body,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync(path, body, JsonOptions, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return ((int)response.StatusCode, content);
    }

    public async Task<(int StatusCode, string Content)> DeleteAsync<T>(
        string path,
        T body,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, path)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return ((int)response.StatusCode, content);
    }
}