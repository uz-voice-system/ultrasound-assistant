using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using UltrasoundAssistant.Contracts.Statistics;
using UltrasoundAssistant.ReportGenerator.Abstractions;

namespace UltrasoundAssistant.ReportGenerator.Services;

public sealed class StatisticsDataClient : IStatisticsDataClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly HttpClient _httpClient;

    public StatisticsDataClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AdminStatisticsDto?> GetAdminStatisticsAsync(AdminStatisticsRequest request, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/read/statistics/admin",
            request,
            JsonOptions,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<AdminStatisticsDto>(JsonOptions, cancellationToken);
    }
}
