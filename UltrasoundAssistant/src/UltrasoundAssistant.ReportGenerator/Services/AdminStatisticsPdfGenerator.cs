using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UltrasoundAssistant.Contracts.Statistics;
using UltrasoundAssistant.ReportGenerator.Abstractions;
using UltrasoundAssistant.ReportGenerator.Options;
using UltrasoundAssistant.ReportGenerator.Services.Localization;

namespace UltrasoundAssistant.ReportGenerator.Services;

public sealed class AdminStatisticsPdfGenerator : IAdminStatisticsPdfGenerator
{
    private readonly CompanyOptions _company;

    public AdminStatisticsPdfGenerator(IOptions<CompanyOptions> company)
    {
        _company = company.Value;
    }

    public byte[] Generate(AdminStatisticsDto statistics)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(c => ComposeContent(c, statistics));
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
            });

            column.Item()
                .PaddingVertical(10)
                .LineHorizontal(1)
                .LineColor(Colors.Grey.Medium);
        });
    }

    private static void ComposeContent(
        IContainer container,
        AdminStatisticsDto statistics)
    {
        container.Column(column =>
        {
            column.Spacing(14);

            column.Item()
                .Text("Статистика администратора")
                .Bold()
                .FontSize(16);

            column.Item()
                .Text($"Период: {FormatDate(statistics.DateFromUtc)} - {FormatDate(statistics.DateToUtc)}")
                .FontSize(11);

            column.Item().Element(c => ComposeSummaryCards(c, statistics));

            column.Item()
                .PaddingTop(5)
                .Text("Статистика по врачам")
                .Bold()
                .FontSize(13);

            column.Item().Element(c => ComposeDoctorsTable(c, statistics.Doctors));

            column.Item()
                .PaddingTop(5)
                .Text("Статистика по шаблонам исследований")
                .Bold()
                .FontSize(13);

            column.Item().Element(c => ComposeTemplatesTable(c, statistics.Templates));

            column.Item()
                .PaddingTop(5)
                .Text("Статистика по статусам отчётов")
                .Bold()
                .FontSize(13);

            column.Item().Element(c => ComposeReportStatusesTable(c, statistics.ReportStatuses));
        });
    }

    private static void ComposeSummaryCards(
    IContainer container,
    AdminStatisticsDto statistics)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Row(row =>
            {
                row.Spacing(8);

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Всего записей",
                    statistics.TotalAppointmentsCount.ToString()));

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Принято пациентов",
                    statistics.AcceptedAppointmentsCount.ToString()));

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Уникальных пациентов",
                    statistics.UniqueAcceptedPatientsCount.ToString()));
            });

            column.Item().Row(row =>
            {
                row.Spacing(8);

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Отчётов создано",
                    statistics.ReportsCount.ToString()));

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Записей без отчёта",
                    statistics.AppointmentsWithoutReportCount.ToString()));

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Процент приёма",
                    FormatPercent(
                        statistics.AcceptedAppointmentsCount,
                        statistics.TotalAppointmentsCount)));
            });
        });
    }

    private static void ComposeMetricCard(
        IContainer container,
        string title,
        string value)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .Background(Colors.Grey.Lighten4)
            .Padding(8)
            .Column(column =>
            {
                column.Item().Text(title).FontSize(9).FontColor(Colors.Grey.Darken2);
                column.Item().Text(value).Bold().FontSize(15);
            });
    }

    private static void ComposeDoctorsTable(
        IContainer container,
        IReadOnlyList<DoctorStatisticsDto> doctors)
    {
        if (doctors.Count == 0)
        {
            container.Text("Нет данных по врачам.");
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Врач");
                header.Cell().Element(HeaderCell).Text("Записей");
                header.Cell().Element(HeaderCell).Text("Принято");
                header.Cell().Element(HeaderCell).Text("Пациентов");
                header.Cell().Element(HeaderCell).Text("Отчётов");
            });

            foreach (var item in doctors)
            {
                table.Cell().Element(Cell).Text(item.DoctorFullName);
                table.Cell().Element(Cell).Text(item.AppointmentsCount.ToString());
                table.Cell().Element(Cell).Text(item.AcceptedAppointmentsCount.ToString());
                table.Cell().Element(Cell).Text(item.UniqueAcceptedPatientsCount.ToString());
                table.Cell().Element(Cell).Text(item.ReportsCount.ToString());
            }
        });
    }

    private static void ComposeTemplatesTable(
        IContainer container,
        IReadOnlyList<TemplateStatisticsDto> templates)
    {
        if (templates.Count == 0)
        {
            container.Text("Нет данных по шаблонам.");
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Исследование");
                header.Cell().Element(HeaderCell).Text("Записей");
                header.Cell().Element(HeaderCell).Text("Принято");
                header.Cell().Element(HeaderCell).Text("Пациентов");
                header.Cell().Element(HeaderCell).Text("Отчётов");
            });

            foreach (var item in templates)
            {
                table.Cell().Element(Cell).Text(item.TemplateName);
                table.Cell().Element(Cell).Text(item.AppointmentsCount.ToString());
                table.Cell().Element(Cell).Text(item.AcceptedAppointmentsCount.ToString());
                table.Cell().Element(Cell).Text(item.UniqueAcceptedPatientsCount.ToString());
                table.Cell().Element(Cell).Text(item.ReportsCount.ToString());
            }
        });
    }

    private static void ComposeReportStatusesTable(
        IContainer container,
        IReadOnlyList<ReportStatusStatisticsDto> statuses)
    {
        if (statuses.Count == 0)
        {
            container.Text("Нет данных по статусам отчётов.");
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Статус");
                header.Cell().Element(HeaderCell).Text("Количество");
            });

            foreach (var item in statuses)
            {
                table.Cell().Element(Cell).Text(ReportDisplayLocalizer.LocalizeReportStatus(item.Status));
                table.Cell().Element(Cell).Text(item.Count.ToString());
            }
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            column.Item()
                .PaddingTop(5)
                .Text("Статистика сформирована автоматически на основании данных записей на приём и отчётов.")
                .FontSize(8)
                .FontColor(Colors.Grey.Darken2)
                .AlignCenter();
        });
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

    private static string FormatDate(DateTime date)
    {
        return date.ToString("dd.MM.yyyy");
    }

    private static string FormatPercent(int value, int total)
    {
        if (total <= 0)
            return "0%";

        var percent = (decimal)value / total * 100;

        return $"{Math.Round(percent, 1)}%";
    }
}
