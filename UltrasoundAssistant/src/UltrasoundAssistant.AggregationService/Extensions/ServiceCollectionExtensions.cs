using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Application.Handlers;
using UltrasoundAssistant.AggregationService.Domain;
using UltrasoundAssistant.AggregationService.Infrastructure.Messaging;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;

namespace UltrasoundAssistant.AggregationService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAggregationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AggregationDb")
                               ?? throw new InvalidOperationException("Connection string 'AggregationDb' is missing");

        services.AddDbContext<AggregationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMq"));

        services.AddScoped<IEventStore, EfCoreEventStore>();
        services.AddScoped<ICommandDeduplicationStore, EfCoreCommandDeduplicationStore>();
        services.AddSingleton<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();
        services.AddScoped<IUnitOfWork, NoOpUnitOfWork>();

        return services;
    }

    public static IServiceCollection AddAggregationApplication(this IServiceCollection services)
    {
        services.AddSingleton<VoiceCommandMatcher>();

        services.AddScoped<PatientCommandHandler>();
        services.AddScoped<TemplateCommandHandler>();
        services.AddScoped<ReportCommandHandler>();

        return services;
    }
}