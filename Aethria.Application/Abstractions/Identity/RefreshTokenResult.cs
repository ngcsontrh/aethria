namespace Aethria.Application.Abstractions.Identity;

public sealed record RefreshTokenResult(
    Guid UserId,
    Guid TokenId,
    string RefreshToken,
    string TokenHash,
    DateTimeOffset ExpiresAt);
