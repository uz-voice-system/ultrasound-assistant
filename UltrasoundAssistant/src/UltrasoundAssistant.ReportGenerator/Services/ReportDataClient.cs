using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using UltrasoundAssistant.Contracts.Reads.Reports.Details;
using UltrasoundAssistant.ReportGenerator.Abstractions;

namespace UltrasoundAssistant.ReportGenerator.Services;

public sealed class ReportDataClient : IReportDataClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly HttpClient _httpClient;

    public ReportDataClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ReportDto?> GetReportAsync(Guid reportId, CancellationToken ct)
    {
        return await _httpClient.GetFromJsonAsync<ReportDto>( $"/api/read/reports/{reportId}", JsonOptions, ct);
    }
}
