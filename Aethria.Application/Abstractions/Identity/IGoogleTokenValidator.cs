namespace Aethria.Application.Abstractions.Identity;

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken);
}

public sealed record GoogleUserInfo(
    string Subject,
    string Email,
    bool EmailVerified);
