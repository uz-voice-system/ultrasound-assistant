using UltrasoundAssistant.Contracts.Auth;

namespace UltrasoundAssistant.ProjectionService.Application.Abstractions;

public interface IAuthReadService
{
    Task<AuthUserDto?> VerifyCredentialsAsync(LoginRequest request, CancellationToken cancellationToken);
}
