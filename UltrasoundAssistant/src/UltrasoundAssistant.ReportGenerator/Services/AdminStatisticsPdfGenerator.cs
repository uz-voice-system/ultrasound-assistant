using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UltrasoundAssistant.Contracts.Statistics;
using UltrasoundAssistant.ReportGenerator.Abstractions;
using UltrasoundAssistant.ReportGenerator.Options;

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
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

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
                .Text("Статистика по статусам записей")
                .Bold()
                .FontSize(13);

            column.Item().Element(c => ComposeAppointmentStatusesTable(c, statistics.AppointmentStatuses));

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
                    "Принято",
                    statistics.AcceptedAppointmentsCount.ToString()));

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Уникальных пациентов",
                    statistics.UniqueAcceptedPatientsCount.ToString()));

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Отчётов",
                    statistics.ReportsCount.ToString()));
            });

            column.Item().Row(row =>
            {
                row.Spacing(8);

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Запланировано",
                    statistics.ScheduledAppointmentsCount.ToString()));

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "В процессе",
                    statistics.InProgressAppointmentsCount.ToString()));

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Завершено",
                    statistics.CompletedAppointmentsCount.ToString()));

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Не явились",
                    statistics.NoShowAppointmentsCount.ToString()));
            });

            column.Item().Row(row =>
            {
                row.Spacing(8);

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Процент приёма",
                    FormatPercent(
                        statistics.AcceptedAppointmentsCount,
                        statistics.TotalAppointmentsCount)));

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Процент завершения",
                    FormatPercent(
                        statistics.CompletedAppointmentsCount,
                        statistics.TotalAppointmentsCount)));

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Процент неявок",
                    FormatPercent(
                        statistics.NoShowAppointmentsCount,
                        statistics.TotalAppointmentsCount)));

                row.RelativeItem().Element(c => ComposeMetricCard(
                    c,
                    "Принято без отчёта",
                    statistics.AppointmentsWithoutReportCount.ToString()));
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
                column.Item().Text(title).FontSize(8).FontColor(Colors.Grey.Darken2);
                column.Item().Text(value).Bold().FontSize(14);
            });
    }

    private static void ComposeAppointmentStatusesTable(
        IContainer container,
        IReadOnlyList<AppointmentStatusStatisticsDto> statuses)
    {
        if (statuses.Count == 0)
        {
            container.Text("Нет данных по статусам записей.");
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
                header.Cell().Element(HeaderCell).Text("Статус записи");
                header.Cell().Element(HeaderCell).Text("Количество");
            });

            foreach (var item in statuses)
            {
                table.Cell().Element(Cell).Text(GetAppointmentStatusName(item));
                table.Cell().Element(Cell).Text(item.Count.ToString());
            }
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
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Врач");
                header.Cell().Element(HeaderCell).Text("Всего");
                header.Cell().Element(HeaderCell).Text("Принято");
                header.Cell().Element(HeaderCell).Text("В процессе");
                header.Cell().Element(HeaderCell).Text("Завершено");
                header.Cell().Element(HeaderCell).Text("Неявки");
                header.Cell().Element(HeaderCell).Text("Пациентов");
                header.Cell().Element(HeaderCell).Text("Отчётов");
            });

            foreach (var item in doctors)
            {
                table.Cell().Element(Cell).Text(item.DoctorFullName);
                table.Cell().Element(Cell).Text(item.AppointmentsCount.ToString());
                table.Cell().Element(Cell).Text(item.AcceptedAppointmentsCount.ToString());
                table.Cell().Element(Cell).Text(item.InProgressAppointmentsCount.ToString());
                table.Cell().Element(Cell).Text(item.CompletedAppointmentsCount.ToString());
                table.Cell().Element(Cell).Text(item.NoShowAppointmentsCount.ToString());
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
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Исследование");
                header.Cell().Element(HeaderCell).Text("Всего");
                header.Cell().Element(HeaderCell).Text("Принято");
                header.Cell().Element(HeaderCell).Text("В процессе");
                header.Cell().Element(HeaderCell).Text("Завершено");
                header.Cell().Element(HeaderCell).Text("Неявки");
                header.Cell().Element(HeaderCell).Text("Пациентов");
                header.Cell().Element(HeaderCell).Text("Отчётов");
            });

            foreach (var item in templates)
            {
                table.Cell().Element(Cell).Text(item.TemplateName);
                table.Cell().Element(Cell).Text(item.AppointmentsCount.ToString());
                table.Cell().Element(Cell).Text(item.AcceptedAppointmentsCount.ToString());
                table.Cell().Element(Cell).Text(item.InProgressAppointmentsCount.ToString());
                table.Cell().Element(Cell).Text(item.CompletedAppointmentsCount.ToString());
                table.Cell().Element(Cell).Text(item.NoShowAppointmentsCount.ToString());
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
                header.Cell().Element(HeaderCell).Text("Статус отчёта");
                header.Cell().Element(HeaderCell).Text("Количество");
            });

            foreach (var item in statuses)
            {
                table.Cell().Element(Cell).Text(GetReportStatusName(item));
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
            .Padding(4);
    }

    private static IContainer Cell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .Padding(4);
    }

    private static string GetAppointmentStatusName(AppointmentStatusStatisticsDto item)
    {
        return !string.IsNullOrWhiteSpace(item.StatusDisplayName)
            ? item.StatusDisplayName
            : item.Status;
    }

    private static string GetReportStatusName(ReportStatusStatisticsDto item)
    {
        return !string.IsNullOrWhiteSpace(item.StatusDisplayName)
            ? item.StatusDisplayName
            : item.Status;
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
