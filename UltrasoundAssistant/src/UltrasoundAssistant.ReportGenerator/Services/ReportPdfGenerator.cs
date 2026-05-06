using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;
using UltrasoundAssistant.Contracts.Entity.Templates;
using UltrasoundAssistant.Contracts.Reads.Reports.Details;
using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.ReportGenerator.Abstractions;
using UltrasoundAssistant.ReportGenerator.Options;

namespace UltrasoundAssistant.ReportGenerator.Services;

public sealed class ReportPdfGenerator : IReportPdfGenerator
{
    private readonly CompanyOptions _company;

    public ReportPdfGenerator(IOptions<CompanyOptions> company)
    {
        _company = company.Value;
    }

    public byte[] Generate(ReportDto report, TemplateDto? template)
    {
        var content = NormalizeContent(report.ContentJson);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c));
                page.Content().Element(c => ComposeContent(c, report, template, content));
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().AlignRight().Column(company =>
            {
                company.Item().Text(_company.Name).Bold().FontSize(12);
                company.Item().Text(_company.Address);
                company.Item().Text($"E-mail: {_company.Email}");
                company.Item().Text($"Тел.: {_company.Phone}");
                company.Item().Text($"Сайт: {_company.Website}");
                company.Item().Text($"Аппарат: {_company.DeviceName}");
            });

            column.Item()
                .PaddingVertical(10)
                .LineHorizontal(1)
                .LineColor(Colors.Grey.Medium);
        });
    }

    private void ComposeContent(
        IContainer container,
        ReportDto report,
        TemplateDto? template,
        IReadOnlyDictionary<string, string> content)
    {
        container.Column(column =>
        {
            column.Spacing(12);

            column.Item().Element(c => ComposePatientInfo(c, report));
            column.Item().Element(c => ComposeStudyInfo(c, report));

            column.Item()
                .PaddingTop(8)
                .Text(report.TemplateName ?? template?.Name ?? "Ультразвуковое исследование")
                .Bold()
                .FontSize(15);

            column.Item()
                .PaddingTop(5)
                .Text("Результат исследования")
                .Bold()
                .FontSize(13);

            column.Item().Element(c => ComposeTemplateFields(c, template, content));

            var description = GetAutoFieldValue(
                template,
                content,
                TemplateFieldRole.Description,
                fallbackFieldName: "description");

            if (!string.IsNullOrWhiteSpace(description))
            {
                column.Item().PaddingTop(10).Text("Описание").Bold().FontSize(13);
                column.Item()
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .PaddingBottom(8)
                    .Text(description);
            }

            var conclusion = GetAutoFieldValue(
                template,
                content,
                TemplateFieldRole.Conclusion,
                fallbackFieldName: "conclusion");

            if (!string.IsNullOrWhiteSpace(conclusion))
            {
                column.Item().PaddingTop(10).Text("Заключение").Bold().FontSize(13);
                column.Item()
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .PaddingBottom(8)
                    .Text(conclusion);
            }

            var recommendation = GetContentValue(content, "recommendation");

            if (!string.IsNullOrWhiteSpace(recommendation))
            {
                column.Item().Text("Рекомендации").Bold().FontSize(13);
                column.Item()
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .PaddingBottom(8)
                    .Text(recommendation);
            }

            column.Item().Element(ComposeUltrasoundImagePlaceholder);

            column.Item().PaddingTop(25).AlignRight().Column(doctor =>
            {
                doctor.Item().Text("Исследование провёл:");
                doctor.Item().Text($"Врач-диагност: {report.DoctorFullName ?? "—"}").Bold();
                doctor.Item().PaddingTop(20).Text("_____________________");
                doctor.Item().Text("подпись");
            });
        });
    }

    private static void ComposePatientInfo(
        IContainer container,
        ReportDto report)
    {
        container.Column(patient =>
        {
            patient.Spacing(2);

            patient.Item().Text($"Пациент: {report.PatientFullName ?? "—"}").Bold();
            patient.Item().Text($"Дата рождения: {FormatDate(report.PatientBirthDate)}");
            patient.Item().Text($"Пол: {FormatGender(report.PatientGender)}");
        });
    }

    private static void ComposeStudyInfo(
        IContainer container,
        ReportDto report)
    {
        container.Column(study =>
        {
            study.Spacing(2);

            study.Item().Text($"Дата исследования: {FormatDateTime(report.AppointmentStartAtUtc ?? report.CreatedAtUtc)}");
            study.Item().Text($"Номер отчёта: {report.Id}");
            study.Item().Text($"Номер записи: {report.AppointmentId}");
        });
    }

    private static void ComposeTemplateFields(
        IContainer container,
        TemplateDto? template,
        IReadOnlyDictionary<string, string> content)
    {
        if (template is null)
        {
            ComposeFlatFields(container, content);
            return;
        }

        container.Column(column =>
        {
            column.Spacing(8);

            foreach (var block in template.Blocks.OrderBy(x => x.Position))
            {
                var regularFields = block.Fields
                    .Where(x => x.Role == TemplateFieldRole.Regular)
                    .OrderBy(x => x.Position)
                    .ToList();

                if (regularFields.Count == 0)
                    continue;

                column.Item().Text(block.Name).Bold().FontSize(12);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Показатель");
                        header.Cell().Element(HeaderCell).Text("Результат");
                        header.Cell().Element(HeaderCell).Text("Норма");
                    });

                    foreach (var field in regularFields)
                    {
                        var value = GetContentValue(content, field.FieldName);

                        table.Cell().Element(Cell).Text(field.DisplayName);
                        table.Cell().Element(Cell).Text(string.IsNullOrWhiteSpace(value) ? "—" : value);
                        table.Cell().Element(Cell).Text(FormatNorm(field.Norm));
                    }
                });
            }

            var extraFields = GetExtraFields(template, content);

            if (extraFields.Count > 0)
            {
                column.Item().PaddingTop(5).Text("Дополнительные поля").Bold().FontSize(12);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Поле");
                        header.Cell().Element(HeaderCell).Text("Значение");
                    });

                    foreach (var field in extraFields.OrderBy(x => x.Key))
                    {
                        table.Cell().Element(Cell).Text(field.Key);
                        table.Cell().Element(Cell).Text(string.IsNullOrWhiteSpace(field.Value) ? "—" : field.Value);
                    }
                });
            }
        });
    }

    private static void ComposeFlatFields(
        IContainer container,
        IReadOnlyDictionary<string, string> content)
    {
        var fields = content
            .Where(x => !IsServiceField(x.Key))
            .OrderBy(x => x.Key)
            .ToList();

        if (fields.Count == 0)
        {
            container.Text("Нет заполненных полей.");
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Показатель");
                header.Cell().Element(HeaderCell).Text("Результат");
            });

            foreach (var field in fields)
            {
                table.Cell().Element(Cell).Text(field.Key);
                table.Cell().Element(Cell).Text(string.IsNullOrWhiteSpace(field.Value) ? "—" : field.Value);
            }
        });
    }

    private static void ComposeUltrasoundImagePlaceholder(IContainer container)
    {
        container.Column(column =>
        {
            column.Item()
                .PaddingTop(15)
                .Text("Изображение исследования")
                .Bold()
                .FontSize(13);

            column.Item()
                .Height(180)
                .Border(1)
                .BorderColor(Colors.Grey.Medium)
                .Background(Colors.Grey.Lighten4)
                .AlignCenter()
                .AlignMiddle()
                .Text("Изображение УЗИ будет добавлено позже")
                .FontColor(Colors.Grey.Darken1)
                .FontSize(12);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            column.Item()
                .PaddingTop(5)
                .Text("Данное заключение не является диагнозом и должно быть интерпретировано лечащим врачом в комплексе с клиническими данными, анамнезом и результатами других исследований.")
                .FontSize(8)
                .FontColor(Colors.Grey.Darken2)
                .AlignCenter();
        });
    }

    private static IReadOnlyDictionary<string, string> NormalizeContent(string? contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                contentJson,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            if (raw is null || raw.Count == 0)
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            return raw
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .GroupBy(x => x.Key.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    x => x.Key,
                    x => ConvertJsonElementToString(x.Last().Value),
                    StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static string ConvertJsonElementToString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => string.Empty,
            JsonValueKind.Undefined => string.Empty,
            _ => element.GetRawText()
        };
    }

    private static string? GetAutoFieldValue(
        TemplateDto? template,
        IReadOnlyDictionary<string, string> content,
        TemplateFieldRole role,
        string fallbackFieldName)
    {
        if (template is not null)
        {
            var field = template.Blocks
                .SelectMany(x => x.Fields)
                .FirstOrDefault(x => x.Role == role);

            if (field is not null)
            {
                var value = GetContentValue(content, field.FieldName);

                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        return GetContentValue(content, fallbackFieldName);
    }

    private static string? GetContentValue(
        IReadOnlyDictionary<string, string> content,
        string fieldName)
    {
        return content.TryGetValue(fieldName, out var value)
            ? value
            : null;
    }

    private static List<KeyValuePair<string, string>> GetExtraFields(
        TemplateDto template,
        IReadOnlyDictionary<string, string> content)
    {
        var templateFieldNames = template.Blocks
            .SelectMany(x => x.Fields)
            .Select(x => x.FieldName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return content
            .Where(x => !templateFieldNames.Contains(x.Key))
            .Where(x => !IsServiceField(x.Key))
            .ToList();
    }

    private static string FormatNorm(FieldNormDto? norm)
    {
        if (norm is null)
            return "—";

        if (!string.IsNullOrWhiteSpace(norm.NormalText))
            return norm.NormalText;

        var unit = string.IsNullOrWhiteSpace(norm.Unit)
            ? string.Empty
            : $" {norm.Unit.Trim()}";

        if (norm.Min is not null && norm.Max is not null)
            return $"{norm.Min}-{norm.Max}{unit}";

        if (norm.Min is not null)
            return $"от {norm.Min}{unit}";

        if (norm.Max is not null)
            return $"до {norm.Max}{unit}";

        return "—";
    }

    private static bool IsServiceField(string fieldName)
    {
        return fieldName.Equals("description", StringComparison.OrdinalIgnoreCase)
               || fieldName.Equals("conclusion", StringComparison.OrdinalIgnoreCase)
               || fieldName.Equals("recommendation", StringComparison.OrdinalIgnoreCase);
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Medium)
            .Background(Colors.Grey.Lighten3)
            .Padding(5);
    }

    private static IContainer Cell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .Padding(5);
    }

    private static string FormatDate(DateTime? date)
    {
        return date.HasValue
            ? date.Value.ToString("dd.MM.yyyy")
            : "—";
    }

    private static string FormatDateTime(DateTime? date)
    {
        return date.HasValue
            ? date.Value.ToString("dd.MM.yyyy HH:mm")
            : "—";
    }

    private static string FormatGender(string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
            return "—";

        return gender switch
        {
            "Male" => "Мужской",
            "Female" => "Женский",
            _ => gender
        };
    }
}
