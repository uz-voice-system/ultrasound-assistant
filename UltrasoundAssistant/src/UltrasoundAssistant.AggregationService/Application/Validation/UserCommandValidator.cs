using UltrasoundAssistant.Contracts.Commands.Users;
using UltrasoundAssistant.Contracts.Entity.Users;

namespace UltrasoundAssistant.AggregationService.Application.Validation;

public static class UserCommandValidator
{
    private const int MaxLoginLength = 100;
    private const int MinPasswordLength = 6;
    private const int MaxPasswordLength = 200;
    private const int MaxFullNameLength = 300;
    private const int MaxSpecializationLength = 200;
    private const int MaxCabinetLength = 100;
    private const int MaxPhoneExtensionLength = 50;

    public static void Validate(CreateUserCommand command)
    {
        if (command.UserId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        ValidateLogin(command.Login);
        ValidatePassword(command.Password, required: true);
        ValidateFullName(command.FullName);

        if (!Enum.IsDefined(command.Role))
            throw new ArgumentException($"Invalid user role: {command.Role}");

        ValidateDoctorProfile(command.DoctorProfile);
    }

    public static void Validate(UpdateUserCommand command)
    {
        if (command.UserId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");

        ValidateLogin(command.Login);

        if (command.Password is not null)
            ValidatePassword(command.Password, required: false);

        ValidateFullName(command.FullName);

        if (!Enum.IsDefined(command.Role))
            throw new ArgumentException($"Invalid user role: {command.Role}");

        ValidateDoctorProfile(command.DoctorProfile);
    }

    public static void Validate(ActivateUserCommand command)
    {
        if (command.UserId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }

    public static void Validate(DeactivateUserCommand command)
    {
        if (command.UserId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }

    private static void ValidateLogin(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new ArgumentException("Login is required");

        if (login.Trim().Length > MaxLoginLength)
            throw new ArgumentException($"Login length cannot exceed {MaxLoginLength}");
    }

    private static void ValidatePassword(string? password, bool required)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            if (required)
                throw new ArgumentException("Password is required");

            throw new ArgumentException("Password cannot be empty");
        }

        var normalizedPassword = password.Trim();

        if (normalizedPassword.Length < MinPasswordLength)
            throw new ArgumentException($"Password length cannot be less than {MinPasswordLength}");

        if (normalizedPassword.Length > MaxPasswordLength)
            throw new ArgumentException($"Password length cannot exceed {MaxPasswordLength}");
    }

    private static void ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("FullName is required");

        if (fullName.Trim().Length > MaxFullNameLength)
            throw new ArgumentException($"FullName length cannot exceed {MaxFullNameLength}");
    }

    private static void ValidateDoctorProfile(DoctorProfileDto? doctorProfile)
    {
        if (doctorProfile is null)
            return;

        if (!string.IsNullOrWhiteSpace(doctorProfile.Specialization) &&
            doctorProfile.Specialization.Trim().Length > MaxSpecializationLength)
        {
            throw new ArgumentException($"Specialization length cannot exceed {MaxSpecializationLength}");
        }

        if (!string.IsNullOrWhiteSpace(doctorProfile.Cabinet) &&
            doctorProfile.Cabinet.Trim().Length > MaxCabinetLength)
        {
            throw new ArgumentException($"Cabinet length cannot exceed {MaxCabinetLength}");
        }

        if (!string.IsNullOrWhiteSpace(doctorProfile.PhoneExtension) &&
            doctorProfile.PhoneExtension.Trim().Length > MaxPhoneExtensionLength)
        {
            throw new ArgumentException($"PhoneExtension length cannot exceed {MaxPhoneExtensionLength}");
        }
    }
}
