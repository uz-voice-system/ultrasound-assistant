namespace UltrasoundAssistant.ReportGenerator.Options;

public sealed class ServiceEndpointsOptions
{
    public const string SectionName = "Services";

    public string ProjectionBaseUrl { get; set; } = "http://localhost:5001";
}
