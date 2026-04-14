using UltrasoundAssistant.ApiGateway.Options;
using UltrasoundAssistant.ApiGateway.Services;

namespace UltrasoundAssistant.ApiGateway.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceEndpointsOptions>(configuration.GetSection("Services"));
        services.Configure<DemoUsersOptions>(configuration.GetSection("DemoUsers"));

        var aggregationBaseUrl = configuration["Services:AggregationBaseUrl"]
                                 ?? throw new InvalidOperationException("Services:AggregationBaseUrl is missing");

        var projectionBaseUrl = configuration["Services:ProjectionBaseUrl"]
                                ?? throw new InvalidOperationException("Services:ProjectionBaseUrl is missing");

        services.AddHttpClient<AggregationApiClient>(client =>
        {
            client.BaseAddress = new Uri(aggregationBaseUrl);
        });

        services.AddHttpClient<ProjectionApiClient>(client =>
        {
            client.BaseAddress = new Uri(projectionBaseUrl);
        });

        services.AddSingleton<AuthService>();

        return services;
    }
}