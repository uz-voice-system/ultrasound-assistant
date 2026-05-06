using Microsoft.AspNetCore.Identity;
using UltrasoundAssistant.Contracts.Auth;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Users;

namespace UltrasoundAssistant.ProjectionService.Application.Services;

public sealed class AuthReadService : IAuthReadService
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly PasswordHasher<object> _passwordHasher;

    public AuthReadService(IUserReadRepository userReadRepository)
    {
        _userReadRepository = userReadRepository;
        _passwordHasher = new PasswordHasher<object>();
    }

    public async Task<AuthUserDto?> VerifyCredentialsAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await _userReadRepository.GetByLoginAsync(request.Login, cancellationToken);

        if (user is null || !user.IsActive)
            return null;

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            new object(),
            user.PasswordHash,
            request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
            return null;

        return new AuthUserDto
        {
            UserId = user.Id,
            Login = user.Login,
            FullName = user.FullName,
            Role = user.Role
        };
    }
}
