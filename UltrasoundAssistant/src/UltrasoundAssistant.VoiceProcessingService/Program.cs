using UltrasoundAssistant.VoiceProcessingService.Application.Abstractions;
using UltrasoundAssistant.VoiceProcessingService.Application.Validation;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Commands;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Generation;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Keywords;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Numbers;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Text;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Values;
using UltrasoundAssistant.VoiceProcessingService.Options;
using UltrasoundAssistant.VoiceProcessingService.Services;
using UltrasoundAssistant.VoiceProcessingService.Services.Templates;
using UltrasoundAssistant.VoiceProcessingService.Services.Whisper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.Configure<WhisperOptions>(
    builder.Configuration.GetSection("Whisper"));

builder.Services.Configure<ProjectionApiOptions>(
    builder.Configuration.GetSection("ProjectionApi"));

var projectionBaseUrl = builder.Configuration["ProjectionApi:BaseUrl"]
                        ?? throw new InvalidOperationException("ProjectionApi:BaseUrl is missing");

builder.Services.AddHttpClient<ITemplateLookupService, TemplateLookupService>(client =>
{
    client.BaseAddress = new Uri(projectionBaseUrl);
});

builder.Services.AddSingleton<ITextNormalizer, RussianTextNormalizer>();
builder.Services.AddSingleton<ITextSimilarityService, LevenshteinTextSimilarityService>();

builder.Services.AddSingleton<IKeywordMatcher, FuzzyKeywordMatcher>();
builder.Services.AddSingleton<IValueExtractor, ValueExtractor>();
builder.Services.AddSingleton<INumberParser, RussianNumberParser>();
builder.Services.AddSingleton<IValueNormalizer, MedicalValueNormalizer>();

builder.Services.AddScoped<IVoiceProcessingUseCase, VoiceProcessingUseCase>();
builder.Services.AddScoped<IReportAutoTextGenerator, ReportAutoTextGenerator>();
builder.Services.AddScoped<VoiceProcessRequestValidator>();
builder.Services.AddScoped<VoicePauseTextProcessor>();

builder.Services.AddScoped<ITemplateMatchingService, TemplateMatchingService>();
builder.Services.AddScoped<IWhisperTranscriptionService, WhisperTranscriptionService>();
builder.Services.AddSingleton<IWhisperModelManager, WhisperModelManager>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();