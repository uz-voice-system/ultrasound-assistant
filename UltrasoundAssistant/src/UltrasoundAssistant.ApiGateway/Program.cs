using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using UltrasoundAssistant.ApiGateway.Middleware;
using UltrasoundAssistant.ApiGateway.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ServiceEndpointsOptions>(builder.Configuration.GetSection(ServiceEndpointsOptions.SectionName));
builder.Services.PostConfigure<ServiceEndpointsOptions>(opt =>
{
    // Fail fast on misconfiguration (empty base URL breaks relative paths on HttpClient).
    if (string.IsNullOrWhiteSpace(opt.AggregationBaseUrl) || string.IsNullOrWhiteSpace(opt.ProjectionBaseUrl))
        throw new InvalidOperationException("Services:AggregationBaseUrl and Services:ProjectionBaseUrl must be set.");
});

builder.Services.AddHttpClient("Aggregation", (sp, client) =>
{
    var opt = sp.GetRequiredService<IOptions<ServiceEndpointsOptions>>().Value;
    client.BaseAddress = new Uri(opt.AggregationBaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient("Projection", (sp, client) =>
{
    var opt = sp.GetRequiredService<IOptions<ServiceEndpointsOptions>>().Value;
    client.BaseAddress = new Uri(opt.ProjectionBaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ultrasound Assistant API Gateway",
        Version = "v1",
        Description =
            "GET — чтение из Projection (read model). POST/PUT/PATCH/DELETE — команды в Aggregation (event store). " +
            "Опциональный заголовок **X-Command-Id** (UUID) — ключ идемпотентности: повтор запроса с тем же id не создаст дубликат события. " +
            "Если не передан, Gateway сгенерирует новый UUID."
    });
});

var app = builder.Build();

var endpoints = app.Services.GetRequiredService<IOptions<ServiceEndpointsOptions>>().Value;
app.Logger.LogInformation(
    "API Gateway upstream: Aggregation={Aggregation}, Projection={Projection}",
    endpoints.AggregationBaseUrl,
    endpoints.ProjectionBaseUrl);

app.Urls.Add("http://+:8080");

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<GatewayExceptionMiddleware>();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

// Internal HTTP between containers: do not redirect to HTTPS (avoids broken calls when only :8080 is exposed).
app.UseAuthorization();

app.MapControllers();

app.Run();
