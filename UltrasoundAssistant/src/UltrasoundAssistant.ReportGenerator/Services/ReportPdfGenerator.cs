using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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
        var fieldLabels = BuildFieldLabels(template);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c));
                page.Content().Element(c => ComposeContent(c, report, fieldLabels));
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

            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private void ComposeContent(
        IContainer container,
        ReportDto report,
        IReadOnlyDictionary<string, string> fieldLabels)
    {
        container.Column(column =>
        {
            column.Spacing(12);

            column.Item().Column(patient =>
            {
                patient.Item().Text($"Пациент: {report.PatientFullName ?? "—"}").Bold();
                patient.Item().Text("Дата рождения: ДОБАВИТЬ ДАТУ РОЖДЕНИЯ ПАЦИЕНТА");
                patient.Item().Text($"Дата исследования: {FormatDate(report.CreatedAtUtc)}");
            });

            column.Item()
                .PaddingTop(8)
                .Text(report.TemplateName ?? "Ультразвуковое исследование")
                .Bold()
                .FontSize(15);

            column.Item().Text("Результат исследования").Bold().FontSize(13);

            column.Item().Table(table =>
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

                //foreach (var field in report.Content
                //             .Where(x => !IsServiceField(x.Key))
                //             .OrderBy(x => GetFieldLabel(x.Key, fieldLabels)))
                //{
                //    var label = GetFieldLabel(field.Key, fieldLabels);

                //    table.Cell().Element(Cell).Text(label);
                //    table.Cell().Element(Cell).Text(string.IsNullOrWhiteSpace(field.Value) ? "—" : field.Value);
                //}
            });

            column.Item().PaddingTop(10).Text("Заключение").Bold().FontSize(13);
            //column.Item().BorderBottom(1).PaddingBottom(8).Text(GetConclusion(report));

            column.Item().Text("Рекомендации").Bold().FontSize(13);
            //column.Item().BorderBottom(1).PaddingBottom(8).Text(GetRecommendation(report));

            column.Item().PaddingTop(25).AlignRight().Column(doctor =>
            {
                doctor.Item().Text("Исследование провёл:");
                doctor.Item().Text("Врач-диагност: ДОБАВИТЬ ИМЯ ВРАЧА").Bold();
                doctor.Item().PaddingTop(20).Text("_____________________");
                doctor.Item().Text("подпись");
            });
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

    private static Dictionary<string, string> BuildFieldLabels(TemplateDto? template)
    {
        // TODOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private static string GetFieldLabel(string fieldName, IReadOnlyDictionary<string, string> fieldLabels)
    {
        return fieldLabels.TryGetValue(fieldName, out var label)
            ? label
            : fieldName;
    }

    private static bool IsServiceField(string fieldName)
    {
        return fieldName.Equals("conclusion", StringComparison.OrdinalIgnoreCase)
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
        return date.HasValue ? date.Value.ToString("dd.MM.yyyy") : "—";
    }

    //private static string GetConclusion(ReportDto report)
    //{
    //    return report.ContentJson.TryGetValue("conclusion", out var value) && !string.IsNullOrWhiteSpace(value)
    //        ? value
    //        : "Без особенностей.";
    //}

    //private static string GetRecommendation(ReportDto report)
    //{
    //    return report.ContentJson.TryGetValue("recommendation", out var value) && !string.IsNullOrWhiteSpace(value)
    //        ? value
    //        : "Консультация лечащего врача.";
    //}
}
