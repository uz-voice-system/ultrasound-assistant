using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UltrasoundAssistant.ApiGateway.Infrastructure.Persistence;
using UltrasoundAssistant.ApiGateway.Middleware;
using UltrasoundAssistant.ApiGateway.Options;
using UltrasoundAssistant.ApiGateway.Services;

namespace UltrasoundAssistant.ApiGateway.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceEndpointsOptions>(configuration.GetSection("Services"));
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.Configure<AuditOptions>(configuration.GetSection("Audit"));

        var auditConnectionString = configuration.GetConnectionString("AuditDb") 
            ?? throw new InvalidOperationException("ConnectionStrings:AuditDb is missing");

        services.AddDbContext<AuditDbContext>(options =>
        {
            options.UseNpgsql(auditConnectionString);
        });

        var aggregationBaseUrl = configuration["Services:AggregationBaseUrl"]
            ?? throw new InvalidOperationException("Services:AggregationBaseUrl is missing");

        var projectionBaseUrl = configuration["Services:ProjectionBaseUrl"]
            ?? throw new InvalidOperationException("Services:ProjectionBaseUrl is missing");

        var voiceProcessingBaseUrl = configuration["Services:VoiceProcessingBaseUrl"]
            ?? throw new InvalidOperationException("Services:VoiceProcessingBaseUrl is missing");

        var reportGeneratorBaseUrl = configuration["Services:ReportGeneratorBaseUrl"]
            ?? throw new InvalidOperationException("Services:ReportGeneratorBaseUrl is missing");

        services.AddHttpClient<AggregationApiClient>(client =>
        {
            client.BaseAddress = new Uri(aggregationBaseUrl);
        });

        services.AddHttpClient<ProjectionApiClient>(client =>
        {
            client.BaseAddress = new Uri(projectionBaseUrl);
        });

        services.AddHttpClient<VoiceProcessingApiClient>(client =>
        {
            client.BaseAddress = new Uri(voiceProcessingBaseUrl);
        });

        services.AddHttpClient<ReportGeneratorClient>(client =>
        {
            client.BaseAddress = new Uri(reportGeneratorBaseUrl);
        });

        services.AddScoped<AuthService>();

        services.AddJwtAuthentication(configuration);
        services.AddSwaggerWithJwt();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration
            .GetSection("Jwt")
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt options are not configured");

        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
            throw new InvalidOperationException("Jwt:Issuer is missing");

        if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
            throw new InvalidOperationException("Jwt:Audience is missing");

        if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
            throw new InvalidOperationException("Jwt:SecretKey is missing");

        if (jwtOptions.SecretKey.Length < 32)
            throw new InvalidOperationException("Jwt:SecretKey must contain at least 32 characters");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddSwaggerWithJwt(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Ultrasound Assistant API Gateway",
                Version = "v1"
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Введите JWT токен в формате: Bearer {token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    securityScheme,
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
