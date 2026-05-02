using Microsoft.AspNetCore.Builder;
using QuestPDF.Infrastructure;
using UltrasoundAssistant.ReportGenerator.Abstractions;
using UltrasoundAssistant.ReportGenerator.Extensions;
using UltrasoundAssistant.ReportGenerator.Options;
using UltrasoundAssistant.ReportGenerator.Services;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddGatewayServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<CompanyOptions>(
    builder.Configuration.GetSection("Company"));

builder.Services.AddSingleton<IReportPdfGenerator, ReportPdfGenerator>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();