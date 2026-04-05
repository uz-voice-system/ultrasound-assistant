using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.AggregationService.Infrastructure;
using UltrasoundAssistant.AggregationService.Persistence;
using UltrasoundAssistant.AggregationService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EventStoreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("EventStore")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IEventStore, EfEventStore>();
builder.Services.AddScoped<CommandService>();
builder.Services.AddHostedService<MigrationHostedService>();
builder.Services.AddScoped<IMessageBrokerPublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<OutboxDispatcher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();
