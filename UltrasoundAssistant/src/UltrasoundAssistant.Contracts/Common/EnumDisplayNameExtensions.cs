using System.ComponentModel;
using System.Reflection;

namespace UltrasoundAssistant.Contracts.Common;

public static class EnumDisplayNameExtensions
{
    public static string GetDisplayName<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        var member = typeof(TEnum)
            .GetMember(value.ToString())
            .FirstOrDefault();

        var description = member?
            .GetCustomAttribute<DescriptionAttribute>()?
            .Description;

        return string.IsNullOrWhiteSpace(description)
            ? value.ToString()
            : description;
    }

    public static string GetDisplayName<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return "—";

        return Enum.TryParse<TEnum>(value.Trim(), ignoreCase: true, out var parsed)
            ? parsed.GetDisplayName()
            : value;
    }
}
