using UltrasoundAssistant.Contracts.Enums;

namespace UltrasoundAssistant.ReportGenerator.Services.Localization;

public static class ReportDisplayLocalizer
{
    public static string LocalizeReportStatus(ReportStatus status)
    {
        return status switch
        {
            ReportStatus.Draft => "Черновик",
            ReportStatus.Completed => "Завершён",
            _ => status.ToString()
        };
    }

    public static string LocalizeReportStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return "—";

        if (Enum.TryParse<ReportStatus>(status, ignoreCase: true, out var parsed))
            return LocalizeReportStatus(parsed);

        return status;
    }

    public static string LocalizeGender(string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
            return "—";

        return gender switch
        {
            "Male" => "Мужской",
            "Female" => "Женский",
            "Other" => "Другой",
            _ => gender
        };
    }
}
