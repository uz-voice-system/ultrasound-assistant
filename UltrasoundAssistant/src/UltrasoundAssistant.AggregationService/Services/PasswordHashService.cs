using Microsoft.AspNetCore.Identity;

namespace UltrasoundAssistant.AggregationService.Application.Services;

public sealed class PasswordHashService
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(new object(), password);
    }
}
