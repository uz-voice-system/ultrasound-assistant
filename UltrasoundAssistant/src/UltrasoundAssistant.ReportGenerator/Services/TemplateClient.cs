using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.ReportGenerator.Abstractions;

namespace UltrasoundAssistant.ReportGenerator.Services;

public sealed class TemplateClient : ITemplateClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly HttpClient _http;

    public TemplateClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<TemplateDto?> GetAsync(Guid templateId, CancellationToken ct)
    {
        return await _http.GetFromJsonAsync<TemplateDto>($"/api/read/templates/{templateId}", JsonOptions, ct);
    }
}
