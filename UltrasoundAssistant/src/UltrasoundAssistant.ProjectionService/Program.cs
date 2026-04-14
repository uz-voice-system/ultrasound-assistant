using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.ProjectionService.Extensions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProjectionInfrastructure(builder.Configuration);
builder.Services.AddProjectionApplication();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting ReadDb migration...");

    var dbContext = scope.ServiceProvider.GetRequiredService<ProjectionDbContext>();
    dbContext.Database.Migrate();

    logger.LogInformation("ReadDb migration completed.");
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();