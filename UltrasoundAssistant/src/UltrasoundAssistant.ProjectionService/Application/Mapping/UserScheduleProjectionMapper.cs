using UltrasoundAssistant.Contracts.Reads.Schedules.Details;
using UltrasoundAssistant.Contracts.Reads.Schedules.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

namespace UltrasoundAssistant.ProjectionService.Application.Mapping;

public sealed class UserScheduleProjectionMapper
{
    public UserScheduleDto MapFull(UserScheduleReadModel model)
    {
        return new UserScheduleDto
        {
            Id = model.Id,
            UserId = model.UserId,
            DayOfWeek = model.DayOfWeek,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            IsDeleted = model.IsDeleted,
            Version = model.Version
        };
    }

    public UserScheduleSummaryDto MapSummary(UserScheduleReadModel model)
    {
        return new UserScheduleSummaryDto
        {
            Id = model.Id,
            UserId = model.UserId,
            UserFullName = model.User.FullName,
            DayOfWeek = model.DayOfWeek,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            IsDeleted = model.IsDeleted,
            Version = model.Version
        };
    }
}