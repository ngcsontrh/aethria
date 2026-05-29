namespace Aethria.Application.Abstractions.Identity;

public interface IAuthTokenService
{
    AccessTokenResult CreateAccessToken(AuthUser user);

    RefreshTokenResult CreateRefreshToken(AuthUser user);

    bool TryGetRefreshTokenId(string refreshToken, out Guid refreshTokenId);

    bool RefreshTokenMatches(string storedHash, string refreshToken);
}

public sealed record AccessTokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAt);

public sealed record RefreshTokenResult(
    Guid UserId,
    Guid TokenId,
    string RefreshToken,
    string TokenHash,
    DateTimeOffset ExpiresAt);
