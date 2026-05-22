namespace Aethria.Application.Abstractions.Identity;

public sealed record GoogleUserInfo(
    string Subject,
    string Email,
    bool EmailVerified);
