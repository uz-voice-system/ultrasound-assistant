using UltrasoundAssistant.Contracts.Entity.Users;
using UltrasoundAssistant.Contracts.Reads.Users.Details;
using UltrasoundAssistant.Contracts.Reads.Users.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

namespace UltrasoundAssistant.ProjectionService.Application.Mapping;

public sealed class UserProjectionMapper
{
    public UserDto MapFull(UserReadModel model)
    {
        return new UserDto
        {
            Id = model.Id,
            Login = model.Login,
            FullName = model.FullName,
            Role = model.Role,
            IsActive = model.IsActive,
            Version = model.Version,
            DoctorProfile = model.DoctorProfile is null ? null : MapDoctorProfile(model.DoctorProfile)
        };
    }

    public UserSummaryDto MapSummary(UserReadModel model)
    {
        return new UserSummaryDto
        {
            Id = model.Id,
            Login = model.Login,
            FullName = model.FullName,
            Role = model.Role,
            IsActive = model.IsActive,
            Cabinet = model.DoctorProfile?.Cabinet,
            Specialization = model.DoctorProfile?.Specialization,
            Version = model.Version
        };
    }

    private static DoctorProfileDto MapDoctorProfile(DoctorProfileReadModel model)
    {
        return new DoctorProfileDto
        {
            UserId = model.UserId,
            Specialization = model.Specialization,
            Cabinet = model.Cabinet,
            PhoneExtension = model.PhoneExtension
        };
    }
}
