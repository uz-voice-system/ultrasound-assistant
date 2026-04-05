using Microsoft.EntityFrameworkCore;
using UltrasoundAssistant.AuditService;
using UltrasoundAssistant.AuditService.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuditDb")));

builder.Services.AddHostedService<MigrationHostedService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
