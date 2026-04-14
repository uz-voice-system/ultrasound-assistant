namespace UltrasoundAssistant.ApiGateway.Services;

public sealed class ProjectionApiClient
{
    private readonly HttpClient _httpClient;

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
}