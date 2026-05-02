using System.Net.Http;

namespace UltrasoundAssistant.ApiGateway.Services;

public sealed class ReportGeneratorClient
{
    private readonly HttpClient _httpClient;

    public ReportGeneratorClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> GetPdfAsync(Guid reportId, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync($"/api/generated-reports/{reportId}/pdf", ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"ReportGenerator error: {response.StatusCode}");
        }

        return await response.Content.ReadAsByteArrayAsync(ct);
    }
}
