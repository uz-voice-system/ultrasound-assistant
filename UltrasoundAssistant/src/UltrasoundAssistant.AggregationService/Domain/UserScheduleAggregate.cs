using System.Text.Json;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;
using UltrasoundAssistant.Contracts.Entity.Schedules;
using UltrasoundAssistant.Contracts.Events.ScheduleEvent;

namespace UltrasoundAssistant.AggregationService.Domain;

public sealed class UserScheduleAggregate
{
    public Guid UserId { get; private set; }

    public bool Exists { get; private set; }

    public int Version { get; private set; }

    public List<UserScheduleItemDto> Items { get; private set; } = [];

    public UserScheduleUpdatedEvent Update(
        Guid userId,
        IReadOnlyList<UserScheduleItemDto> items)
    {
        return new UserScheduleUpdatedEvent
        {
            UserId = userId,
            Items = CloneItems(items),
            Version = Version + 1
        };
    }

    public void LoadFrom(IEnumerable<EventRecord> history)
    {
        foreach (var item in history.OrderBy(x => x.Version))
            Apply(item);
    }

    private void Apply(EventRecord record)
    {
        switch (record.EventType)
        {
            case nameof(UserScheduleUpdatedEvent):
                {
                    var e = JsonSerializer.Deserialize<UserScheduleUpdatedEvent>(
                                record.Payload,
                                JsonDefaults.Web)
                            ?? throw new InvalidOperationException("Invalid UserScheduleUpdatedEvent payload");

                    UserId = e.UserId;
                    Items = CloneItems(e.Items);
                    Exists = true;
                    Version = e.Version;
                    break;
                }
        }
    }

    private static List<UserScheduleItemDto> CloneItems(
        IReadOnlyList<UserScheduleItemDto> items)
    {
        return items
            .Select(item => new UserScheduleItemDto
            {
                ScheduleId = item.ScheduleId,
                DayOfWeek = item.DayOfWeek,
                StartTime = item.StartTime,
                EndTime = item.EndTime
            })
            .ToList();
    }
}
