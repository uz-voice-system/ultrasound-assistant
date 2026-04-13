using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.AggregationService.Extensions;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAggregationInfrastructure(builder.Configuration);
builder.Services.AddAggregationApplication();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var dbContext = scope.ServiceProvider.GetRequiredService<AggregationDbContext>();
    logger.LogInformation("Starting database migration...");
    dbContext.Database.Migrate();
    logger.LogInformation("Database migration completed.");
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();