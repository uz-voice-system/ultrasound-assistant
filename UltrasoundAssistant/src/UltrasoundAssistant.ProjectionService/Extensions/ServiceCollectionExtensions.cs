using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.EventHandlers.Appointments;
using UltrasoundAssistant.ProjectionService.Application.EventHandlers.Patients;
using UltrasoundAssistant.ProjectionService.Application.EventHandlers.Reports;
using UltrasoundAssistant.ProjectionService.Application.EventHandlers.Schedules;
using UltrasoundAssistant.ProjectionService.Application.EventHandlers.Templates;
using UltrasoundAssistant.ProjectionService.Application.EventHandlers.Users;
using UltrasoundAssistant.ProjectionService.Application.Mapping;
using UltrasoundAssistant.ProjectionService.Application.Services;
using UltrasoundAssistant.ProjectionService.Consumers;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Messaging;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Repositories;

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
        services.AddScoped<IPatientReadRepository, PatientReadRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IUserScheduleReadRepository, UserScheduleReadRepository>();
        services.AddScoped<IAppointmentReadRepository, AppointmentReadRepository>();
        services.AddScoped<IReportReadRepository, ReportReadRepository>();
        services.AddScoped<ITemplateReadRepository, TemplateReadRepository>();

        services.AddScoped<IPatientReadService, PatientReadService>();
        services.AddScoped<IUserReadService, UserReadService>();
        services.AddScoped<IUserScheduleReadService, UserScheduleReadService>();
        services.AddScoped<IAppointmentReadService, AppointmentReadService>();
        services.AddScoped<IReportReadService, ReportReadService>();
        services.AddScoped<ITemplateReadService, TemplateReadService>();
        services.AddScoped<IAuthReadService, AuthReadService>();

        services.AddScoped<PatientProjectionMapper>();
        services.AddScoped<UserProjectionMapper>();
        services.AddScoped<UserScheduleProjectionMapper>();
        services.AddScoped<AppointmentProjectionMapper>();
        services.AddScoped<ReportProjectionMapper>();
        services.AddScoped<TemplateProjectionMapper>();

        services.AddScoped<IIntegrationEventHandler, PatientCreatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, PatientUpdatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, PatientDeletedEventHandler>();

        services.AddScoped<IIntegrationEventHandler, UserCreatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, UserUpdatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, UserActivatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, UserDeactivatedEventHandler>();

        services.AddScoped<IIntegrationEventHandler, UserScheduleUpdatedEventHandler>();

        services.AddScoped<IIntegrationEventHandler, AppointmentCreatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, AppointmentUpdatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, AppointmentDeletedEventHandler>();

        services.AddScoped<IIntegrationEventHandler, ReportCreatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, ReportUpdatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, ReportDeletedEventHandler>();

        services.AddScoped<IIntegrationEventHandler, TemplateCreatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, TemplateUpdatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler, TemplateDeletedEventHandler>();

        services.AddHostedService<RabbitMqProjectionConsumer>();

        return services;
    }
}
