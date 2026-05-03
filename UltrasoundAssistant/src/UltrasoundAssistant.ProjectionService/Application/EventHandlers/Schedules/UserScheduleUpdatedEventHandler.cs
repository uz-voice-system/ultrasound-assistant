using UltrasoundAssistant.Contracts.Events.ScheduleEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Schedules;

public sealed class UserScheduleUpdatedEventHandler : IIntegrationEventHandler
{
    private readonly IUserScheduleReadRepository _repository;

    public string RoutingKey => "user_schedule.updated";

    public UserScheduleUpdatedEventHandler(IUserScheduleReadRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = EventPayloadReader.Read<UserScheduleUpdatedEvent>(payload, nameof(UserScheduleUpdatedEvent));

        var existingSchedules = await _repository.GetByUserIdForUpdateAsync(
            @event.UserId,
            cancellationToken);

        if (existingSchedules.Count > 0 && @event.Version <= existingSchedules.Max(x => x.Version))
            return;

        var incomingIds = @event.Items
            .Select(x => x.ScheduleId)
            .ToHashSet();

        foreach (var existingSchedule in existingSchedules)
        {
            if (!incomingIds.Contains(existingSchedule.Id))
            {
                existingSchedule.IsDeleted = true;
                existingSchedule.Version = @event.Version;
            }
        }

        foreach (var item in @event.Items)
        {
            var schedule = existingSchedules
                .FirstOrDefault(x => x.Id == item.ScheduleId);

            if (schedule is null)
            {
                schedule = new UserScheduleReadModel
                {
                    Id = item.ScheduleId,
                    UserId = @event.UserId
                };

                await _repository.AddAsync(schedule, cancellationToken);
            }

            schedule.DayOfWeek = item.DayOfWeek;
            schedule.StartTime = item.StartTime;
            schedule.EndTime = item.EndTime;
            schedule.IsDeleted = false;
            schedule.Version = @event.Version;
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
