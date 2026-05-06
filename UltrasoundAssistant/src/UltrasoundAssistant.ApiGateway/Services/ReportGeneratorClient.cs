using System.Net.Http.Headers;

namespace UltrasoundAssistant.ApiGateway.Services;

public sealed class ReportGeneratorClient
{
    private readonly HttpClient _httpClient;

    public ReportGeneratorClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GeneratedFileResult> GetReportPdfAsync(
        Guid reportId,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            $"/api/generated-reports/{reportId}/pdf",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        var contentType = response.Content.Headers.ContentType?.ToString()
                          ?? "application/octet-stream";

        var fileName = GetFileName(response.Content.Headers.ContentDisposition)
                       ?? $"report-{reportId:N}.pdf";

        return new GeneratedFileResult
        {
            StatusCode = (int)response.StatusCode,
            Content = content,
            ContentType = contentType,
            FileName = fileName
        };
    }

    private static string? GetFileName(ContentDispositionHeaderValue? contentDisposition)
    {
        if (contentDisposition is null)
            return null;

        if (!string.IsNullOrWhiteSpace(contentDisposition.FileNameStar))
            return contentDisposition.FileNameStar.Trim('"');

        if (!string.IsNullOrWhiteSpace(contentDisposition.FileName))
            return contentDisposition.FileName.Trim('"');

        return null;
    }
}

public sealed class GeneratedFileResult
{
    public int StatusCode { get; set; }

    public byte[] Content { get; set; } = [];

    public string ContentType { get; set; } = "application/octet-stream";

    public string FileName { get; set; } = string.Empty;
}
