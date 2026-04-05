using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.ProjectionService.Persistence;
using UltrasoundAssistant.ProjectionService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ReadDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReadDb")));

builder.Services.AddScoped<DomainEventProcessor>();
builder.Services.AddHostedService<MigrationHostedService>();
builder.Services.AddHostedService<RabbitMqProjectionConsumer>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
