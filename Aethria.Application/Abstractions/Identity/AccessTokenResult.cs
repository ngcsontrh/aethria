namespace Aethria.Application.Abstractions.Identity;

public sealed record AccessTokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAt);
