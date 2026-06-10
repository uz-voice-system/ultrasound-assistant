using UltrasoundAssistant.Contracts.Entity.Templates;
using UltrasoundAssistant.Contracts.VoiceProcessing;
using UltrasoundAssistant.Tests.TestDoubles;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Numbers;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Text;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Values;

namespace UltrasoundAssistant.Tests.VoiceProcessing;

public sealed class MedicalValueNormalizerTests
{
    private readonly MedicalValueNormalizer _normalizer;

    public MedicalValueNormalizerTests()
    {
        var similarityService = new TestTextSimilarityService();

        _normalizer = new MedicalValueNormalizer(
            new RussianNumberParser(similarityService),
            new RussianTextNormalizer(),
            similarityService);
    }

    [Theory]
    [InlineData("десять сантиметров", "10 см")]
    [InlineData("10 см", "10 см")]
    [InlineData("10 сантиметра", "10 см")]
    [InlineData("десять см", "10 см")]
    public void Normalize_NumberWithUnit_ShouldReturnUnifiedValue(
        string rawValue,
        string expectedValue)
    {
        var field = new TemplateFieldDto
        {
            FieldName = "right_kidney_length",
            DisplayName = "Длина правой почки",
            Type = TemplateFieldType.NumberWithUnit,
            Norm = new FieldNormDto
            {
                Min = 9,
                Max = 12,
                Unit = "см"
            }
        };

        var result = _normalizer.Normalize(rawValue, field);

        Assert.False(string.IsNullOrWhiteSpace(result.Value));
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal(10m, result.NumericValue);
        Assert.Equal("см", result.Unit);
        Assert.True(result.Confidence > 0.7);
    }

    [Fact]
    public void Normalize_EmptyValue_ShouldFail()
    {
        var field = new TemplateFieldDto
        {
            FieldName = "right_kidney_length",
            DisplayName = "Длина правой почки",
            Type = TemplateFieldType.NumberWithUnit,
            Norm = new FieldNormDto
            {
                Min = 9,
                Max = 12,
                Unit = "см"
            }
        };

        var result = _normalizer.Normalize(string.Empty, field);

        Assert.True(string.IsNullOrWhiteSpace(result.Value));
        Assert.Equal(0, result.Confidence);
    }

    [Fact]
    public void Normalize_NumberWithUnit_WithoutNumber_ShouldReturnUnknownNumericValue()
    {
        var field = new TemplateFieldDto
        {
            FieldName = "right_kidney_length",
            DisplayName = "Длина правой почки",
            Type = TemplateFieldType.NumberWithUnit,
            Norm = new FieldNormDto
            {
                Min = 9,
                Max = 12,
                Unit = "см"
            }
        };

        var result = _normalizer.Normalize("сантиметров", field);

        Assert.Equal("сантиметров", result.Value);
        Assert.Null(result.NumericValue);
        Assert.Equal(NormStatus.Unknown, result.NormStatus);
        Assert.False(string.IsNullOrWhiteSpace(result.NormMessage));
    }

    [Theory]
    [InlineData("8 см", NormStatus.Below)]
    [InlineData("10 см", NormStatus.Normal)]
    [InlineData("16 см", NormStatus.Above)]
    public void Normalize_NumberWithUnit_ShouldCheckNorm(
        string rawValue,
        NormStatus expectedStatus)
    {
        var field = new TemplateFieldDto
        {
            FieldName = "right_kidney_length",
            DisplayName = "Длина правой почки",
            Type = TemplateFieldType.NumberWithUnit,
            Norm = new FieldNormDto
            {
                Min = 9,
                Max = 12,
                Unit = "см"
            }
        };

        var result = _normalizer.Normalize(rawValue, field);

        Assert.Equal(expectedStatus, result.NormStatus);
    }

    [Fact]
    public void Normalize_TextDifferentFromNorm_ShouldNotReduceRecognitionConfidence()
    {
        var field = new TemplateFieldDto
        {
            FieldName = "right_contour",
            DisplayName = "Контуры правой почки",
            Type = TemplateFieldType.Text,
            Norm = new FieldNormDto
            {
                NormalText = "Ровные, чёткие"
            }
        };

        var result = _normalizer.Normalize("неровный", field);

        Assert.Equal("неровный", result.Value);
        Assert.Equal(NormStatus.AbnormalText, result.NormStatus);
        Assert.True(result.Confidence >= 0.9);
    }
}
