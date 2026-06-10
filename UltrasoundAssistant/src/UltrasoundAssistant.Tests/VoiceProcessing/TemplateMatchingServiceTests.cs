using UltrasoundAssistant.Contracts.Entity.Templates;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.Contracts.VoiceProcessing;
using UltrasoundAssistant.Tests.TestDoubles;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Commands;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Generation;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Numbers;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Text;
using UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Values;
using UltrasoundAssistant.VoiceProcessingService.Services.Templates;

namespace UltrasoundAssistant.Tests.VoiceProcessing;

public sealed class TemplateMatchingServiceTests
{
    private readonly TemplateMatchingService _service;

    public TemplateMatchingServiceTests()
    {
        var textNormalizer = new RussianTextNormalizer();
        var similarityService = new TestTextSimilarityService();

        _service = new TemplateMatchingService(
            textNormalizer,
            new TestKeywordMatcher(textNormalizer),
            new ValueExtractor(),
            new MedicalValueNormalizer(
                new RussianNumberParser(similarityService),
                textNormalizer,
                similarityService),
            new ReportAutoTextGenerator(),
            new VoicePauseTextProcessor());
    }

    [Fact]
    public void Match_KnownKeywords_ShouldFillExpectedFields()
    {
        var template = CreateKidneyTemplate();

        var result = _service.Match(
            "Правая почка размер 10 см. Поперечный размер 5 см. Толщина 4 см. Паренхима 3 см.",
            template);

        Assert.True(result.Matched);
        Assert.Null(result.Error);

        Assert.Contains(result.Fields, x =>
            x.BlockName == "Правая почка" &&
            x.FieldName == "right_kidney_length" &&
            x.Value == "10 см");

        Assert.Contains(result.Fields, x =>
            x.BlockName == "Правая почка" &&
            x.FieldName == "right_kidney_width" &&
            x.Value == "5 см");

        Assert.Contains(result.Fields, x =>
            x.BlockName == "Правая почка" &&
            x.FieldName == "right_kidney_thickness" &&
            x.Value == "4 см");

        Assert.Contains(result.Fields, x =>
            x.BlockName == "Правая почка" &&
            x.FieldName == "right_parenchyma" &&
            x.Value == "3 см");
    }

    [Fact]
    public void Match_UnknownKeyword_ShouldNotFillWrongField()
    {
        var template = CreateKidneyTemplate();

        VoiceProcessResult? result = null;

        var exception = Record.Exception(() =>
        {
            result = _service.Match(
                "Правая почка неизвестный показатель 123 см.",
                template);
        });

        Assert.Null(exception);
        Assert.NotNull(result);

        Assert.False(result!.Matched);
        Assert.Empty(result.Fields);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public void Match_PauseAndContinue_ShouldIgnoreTextBetweenCommands()
    {
        var template = CreateKidneyTemplate();

        var result = _service.Match(
            "Правая почка положение типичное контур неровный длина 16 см пауза эхогенность обычная левая почка положение типичное продолжить левая почка контур ровный длина 8 см",
            template);

        Assert.True(result.Matched);

        Assert.Contains(result.Fields, x =>
            x.BlockName == "Правая почка" &&
            x.FieldName == "right_contour" &&
            x.Value == "неровный");

        Assert.Contains(result.Fields, x =>
            x.BlockName == "Правая почка" &&
            x.FieldName == "right_kidney_length" &&
            x.Value == "16 см");

        Assert.Contains(result.Fields, x =>
            x.BlockName == "Левая почка" &&
            x.FieldName == "left_contour");

        Assert.DoesNotContain(result.Fields, x =>
            x.FieldName.Contains("echogenicity", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Match_RepeatedField_ShouldKeepLastValue()
    {
        var template = CreateKidneyTemplate();

        var result = _service.Match(
            "Левая почка длина 15 см длина 8 см",
            template);

        var leftLengthFields = result.Fields
            .Where(x => x.BlockName == "Левая почка" &&
                        x.FieldName == "left_kidney_length")
            .ToList();

        Assert.Single(leftLengthFields);
        Assert.Equal("8 см", leftLengthFields[0].Value);
    }

    [Fact]
    public void Match_ExtractedValues_ShouldNotBeReturnedAsUnmatchedParts()
    {
        var template = CreateKidneyTemplate();

        var result = _service.Match(
            "Правая почка положение типичное контур неровный длина 16 см",
            template);

        Assert.True(result.Matched);

        Assert.DoesNotContain("типичное", result.UnmatchedParts);
        Assert.DoesNotContain("неровный", result.UnmatchedParts);
        Assert.DoesNotContain("16 см", result.UnmatchedParts);
    }

    private static TemplateDto CreateKidneyTemplate()
    {
        return new TemplateDto
        {
            Id = Guid.Parse("74000000-0000-0000-0000-000000000001"),
            Name = "УЗИ почек",
            DefaultAppointmentDurationMinutes = 30,
            Blocks =
            [
                new TemplateBlockDto
                {
                    Id = Guid.Parse("74000000-0000-0000-0000-000000000101"),
                    Name = "Правая почка",
                    Position = 1,
                    Phrases =
                    [
                        "правая почка",
                        "почка справа"
                    ],
                    Fields =
                    [
                        new TemplateFieldDto
                        {
                            Id = Guid.Parse("74000000-0000-0000-0000-000000001001"),
                            FieldName = "right_position",
                            DisplayName = "Расположение правой почки",
                            Position = 1,
                            Type = TemplateFieldType.Text,
                            Role = TemplateFieldRole.Regular,
                            Phrases =
                            [
                                "положение",
                                "расположение"
                            ],
                            Norm = new FieldNormDto
                            {
                                NormalText = "Типичное"
                            }
                        },
                        new TemplateFieldDto
                        {
                            Id = Guid.Parse("74000000-0000-0000-0000-000000001002"),
                            FieldName = "right_contour",
                            DisplayName = "Контуры правой почки",
                            Position = 2,
                            Type = TemplateFieldType.Text,
                            Role = TemplateFieldRole.Regular,
                            Phrases =
                            [
                                "контур",
                                "контуры"
                            ],
                            Norm = new FieldNormDto
                            {
                                NormalText = "Ровные, чёткие"
                            }
                        },
                        new TemplateFieldDto
                        {
                            Id = Guid.Parse("74000000-0000-0000-0000-000000001003"),
                            FieldName = "right_kidney_length",
                            DisplayName = "Длина правой почки",
                            Position = 3,
                            Type = TemplateFieldType.NumberWithUnit,
                            Role = TemplateFieldRole.Regular,
                            Phrases =
                            [
                                "размер",
                                "длина",
                                "продольный размер"
                            ],
                            Norm = new FieldNormDto
                            {
                                Min = 9,
                                Max = 12,
                                Unit = "см"
                            }
                        },
                        new TemplateFieldDto
                        {
                            Id = Guid.Parse("74000000-0000-0000-0000-000000001004"),
                            FieldName = "right_kidney_width",
                            DisplayName = "Поперечный размер правой почки",
                            Position = 4,
                            Type = TemplateFieldType.NumberWithUnit,
                            Role = TemplateFieldRole.Regular,
                            Phrases =
                            [
                                "поперечный размер",
                                "поперечному размеру",
                                "ширина"
                            ],
                            Norm = new FieldNormDto
                            {
                                Min = 4,
                                Max = 6,
                                Unit = "см"
                            }
                        },
                        new TemplateFieldDto
                        {
                            Id = Guid.Parse("74000000-0000-0000-0000-000000001005"),
                            FieldName = "right_kidney_thickness",
                            DisplayName = "Толщина правой почки",
                            Position = 5,
                            Type = TemplateFieldType.NumberWithUnit,
                            Role = TemplateFieldRole.Regular,
                            Phrases =
                            [
                                "толщина",
                                "толщин"
                            ],
                            Norm = new FieldNormDto
                            {
                                Min = 3,
                                Max = 5,
                                Unit = "см"
                            }
                        },
                        new TemplateFieldDto
                        {
                            Id = Guid.Parse("74000000-0000-0000-0000-000000001006"),
                            FieldName = "right_parenchyma",
                            DisplayName = "Паренхима правой почки",
                            Position = 6,
                            Type = TemplateFieldType.NumberWithUnit,
                            Role = TemplateFieldRole.Regular,
                            Phrases =
                            [
                                "паренхима",
                                "толщина паренхимы"
                            ],
                            Norm = new FieldNormDto
                            {
                                Min = 1,
                                Max = 3,
                                Unit = "см"
                            }
                        }
                    ]
                },
                new TemplateBlockDto
                {
                    Id = Guid.Parse("74000000-0000-0000-0000-000000000102"),
                    Name = "Левая почка",
                    Position = 2,
                    Phrases =
                    [
                        "левая почка",
                        "почка слева"
                    ],
                    Fields =
                    [
                        new TemplateFieldDto
                        {
                            Id = Guid.Parse("74000000-0000-0000-0000-000000002001"),
                            FieldName = "left_contour",
                            DisplayName = "Контуры левой почки",
                            Position = 1,
                            Type = TemplateFieldType.Text,
                            Role = TemplateFieldRole.Regular,
                            Phrases =
                            [
                                "контур",
                                "контуры"
                            ],
                            Norm = new FieldNormDto
                            {
                                NormalText = "Ровные, чёткие"
                            }
                        },
                        new TemplateFieldDto
                        {
                            Id = Guid.Parse("74000000-0000-0000-0000-000000002002"),
                            FieldName = "left_kidney_length",
                            DisplayName = "Длина левой почки",
                            Position = 2,
                            Type = TemplateFieldType.NumberWithUnit,
                            Role = TemplateFieldRole.Regular,
                            Phrases =
                            [
                                "размер",
                                "длина",
                                "продольный размер"
                            ],
                            Norm = new FieldNormDto
                            {
                                Min = 9,
                                Max = 12,
                                Unit = "см"
                            }
                        }
                    ]
                },
                new TemplateBlockDto
                {
                    Id = Guid.Parse("74000000-0000-0000-0000-000000000103"),
                    Name = "Итоговая часть",
                    Position = 99,
                    Phrases = [],
                    Fields =
                    [
                        new TemplateFieldDto
                        {
                            Id = Guid.Parse("74000000-0000-0000-0000-000000003001"),
                            FieldName = "auto.description",
                            DisplayName = "Описание",
                            Position = 1,
                            Type = TemplateFieldType.Text,
                            Role = TemplateFieldRole.Description,
                            Phrases = []
                        },
                        new TemplateFieldDto
                        {
                            Id = Guid.Parse("74000000-0000-0000-0000-000000003002"),
                            FieldName = "auto.conclusion",
                            DisplayName = "Заключение",
                            Position = 2,
                            Type = TemplateFieldType.Text,
                            Role = TemplateFieldRole.Conclusion,
                            Phrases = []
                        }
                    ]
                }
            ]
        };
    }
}
