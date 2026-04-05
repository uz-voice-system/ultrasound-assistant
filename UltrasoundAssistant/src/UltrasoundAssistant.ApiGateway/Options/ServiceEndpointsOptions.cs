namespace UltrasoundAssistant.ApiGateway.Options;

public sealed class ServiceEndpointsOptions
{
    public const string SectionName = "Services";

    public string AggregationBaseUrl { get; set; } = "http://localhost:5002";

    public string ProjectionBaseUrl { get; set; } = "http://localhost:5001";
}
