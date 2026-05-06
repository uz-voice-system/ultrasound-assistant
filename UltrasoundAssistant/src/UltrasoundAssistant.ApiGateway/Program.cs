using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using UltrasoundAssistant.ApiGateway.Extensions;
using UltrasoundAssistant.ApiGateway.Infrastructure.Persistence;
using UltrasoundAssistant.ApiGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider
        .GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Starting AuditDb migration...");

    var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    dbContext.Database.Migrate();

    logger.LogInformation("AuditDb migration completed.");
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<AuditMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();