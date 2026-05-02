namespace UltrasoundAssistant.ProjectionService.Application.Common;

public static class DateTimeHelper
{
    public static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    public static DateTime? EnsureUtc(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        return EnsureUtc(value.Value);
    }
}
