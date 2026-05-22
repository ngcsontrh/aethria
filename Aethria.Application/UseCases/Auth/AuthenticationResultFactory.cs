namespace Aethria.Application.UseCases.Auth;

internal static class AuthenticationResultFactory
{
    public static async Task<AuthenticationResult> CreateAsync(
        AuthUser user,
        IAuthTokenService authTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var accessToken = authTokenService.CreateAccessToken(user);
        var refreshToken = authTokenService.CreateRefreshToken(user);
        var now = DateTimeOffset.UtcNow;

        await refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = refreshToken.TokenId,
            UserId = refreshToken.UserId,
            TokenHash = refreshToken.TokenHash,
            ExpiresAt = refreshToken.ExpiresAt,
            Status = RefreshTokenStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Create(user, accessToken, refreshToken);
    }

    public static AuthenticationResult Create(
        AuthUser user,
        AccessTokenResult accessToken,
        RefreshTokenResult refreshToken)
    {
        return new AuthenticationResult(
            UserId: user.UserId,
            Email: user.Email,
            AccessToken: accessToken.AccessToken,
            AccessTokenExpiresAt: accessToken.ExpiresAt,
            RefreshToken: refreshToken.RefreshToken,
            RefreshTokenExpiresAt: refreshToken.ExpiresAt);
    }
}
