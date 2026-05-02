using UltrasoundAssistant.ReportGenerator.Abstractions;
using UltrasoundAssistant.ReportGenerator.Options;
using UltrasoundAssistant.ReportGenerator.Services;

namespace UltrasoundAssistant.ReportGenerator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceEndpointsOptions>(configuration.GetSection("Services"));

        var projectionBaseUrl = configuration["Services:ProjectionBaseUrl"]
            ?? throw new InvalidOperationException("Services:ProjectionBaseUrl is missing");

        services.AddHttpClient<IReportDataClient, ReportDataClient>(client =>
        {
            client.BaseAddress = new Uri(projectionBaseUrl);
        });

        services.AddHttpClient<ITemplateClient, TemplateClient>(client =>
        {
            client.BaseAddress = new Uri(projectionBaseUrl);
        });

        return services;
    }
}
