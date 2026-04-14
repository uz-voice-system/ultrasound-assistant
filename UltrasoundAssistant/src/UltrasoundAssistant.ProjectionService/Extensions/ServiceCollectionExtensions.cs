using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.EventHandlers;
using UltrasoundAssistant.ProjectionService.Consumers;
using UltrasoundAssistant.ProjectionService.Infrastructure.Messaging;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

namespace UltrasoundAssistant.ProjectionService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ReadDb")
                               ?? throw new InvalidOperationException("Connection string 'ReadDb' is missing");

        services.AddDbContext<ProjectionDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMq"));

        return services;
    }

    public static IServiceCollection AddProjectionApplication(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventHandler, PatientCreatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, PatientUpdatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, PatientDeactivatedEventHandler>();

        services.AddScoped<IIntegrationEventHandler, TemplateCreatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, TemplateUpdatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, TemplateDeletedEventHandler>();

        services.AddScoped<IIntegrationEventHandler, ReportCreatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, ReportFieldUpdatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, ReportCompletedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, ReportDeletedEventHandler>();

        services.AddHostedService<RabbitMqProjectionConsumer>();

        return services;
    }
}