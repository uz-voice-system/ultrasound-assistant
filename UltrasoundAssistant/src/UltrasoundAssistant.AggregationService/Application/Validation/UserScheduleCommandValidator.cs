using UltrasoundAssistant.Contracts.Commands.Schedules;
using UltrasoundAssistant.Contracts.Entity.Schedules;

namespace UltrasoundAssistant.AggregationService.Application.Validation;

public static class UserScheduleCommandValidator
{
    public static void Validate(UpdateUserScheduleCommand command)
    {
        if (command.UserId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");

        if (command.Items is null)
            throw new ArgumentException("Schedule items are required");

        ValidateItems(command.Items);
    }

    private static void ValidateItems(IReadOnlyList<UserScheduleItemDto> items)
    {
        var ids = new HashSet<Guid>();

        foreach (var item in items)
        {
            if (item.ScheduleId == Guid.Empty)
                throw new ArgumentException("ScheduleId is required");

            if (!ids.Add(item.ScheduleId))
                throw new ArgumentException($"Duplicate schedule id: {item.ScheduleId}");

            if (!Enum.IsDefined(item.DayOfWeek))
                throw new ArgumentException($"Invalid day of week: {item.DayOfWeek}");

            if (item.EndTime <= item.StartTime)
                throw new ArgumentException("EndTime must be greater than StartTime");
        }

        ValidateNoOverlaps(items);
    }

    private static void ValidateNoOverlaps(IReadOnlyList<UserScheduleItemDto> items)
    {
        var groupedByDay = items.GroupBy(x => x.DayOfWeek);

        foreach (var group in groupedByDay)
        {
            var ordered = group
                .OrderBy(x => x.StartTime)
                .ToList();

            for (var i = 1; i < ordered.Count; i++)
            {
                var previous = ordered[i - 1];
                var current = ordered[i];

                if (current.StartTime < previous.EndTime)
                {
                    throw new ArgumentException(
                        $"Schedule intervals overlap for day {group.Key}: {previous.StartTime}-{previous.EndTime} and {current.StartTime}-{current.EndTime}");
                }
            }
        }
    }
}
