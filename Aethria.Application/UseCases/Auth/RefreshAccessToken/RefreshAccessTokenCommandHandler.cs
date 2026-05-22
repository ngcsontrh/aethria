namespace Aethria.Application.UseCases.Auth.RefreshAccessToken;

public sealed class RefreshAccessTokenCommandHandler : IRequestHandler<RefreshAccessTokenCommand, ValueTask<Result<AuthenticationResult>>>
{
    private readonly IIdentityAuthService _identityAuthService;
    private readonly IAuthTokenService _authTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshAccessTokenCommandHandler(
        IIdentityAuthService identityAuthService,
        IAuthTokenService authTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _identityAuthService = identityAuthService;
        _authTokenService = authTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<AuthenticationResult>> Handle(RefreshAccessTokenCommand command, CancellationToken cancellationToken)
    {
        var refreshTokenValue = command.RefreshToken!;

        if (!_authTokenService.TryGetRefreshTokenId(refreshTokenValue, out var refreshTokenId))
        {
            return Result.Fail(new UnauthorizedError("Invalid refresh token."));
        }

        var storedRefreshToken = await _refreshTokenRepository.GetActiveByIdAsync(refreshTokenId, cancellationToken);
        if (storedRefreshToken is null
            || !_authTokenService.RefreshTokenMatches(storedRefreshToken.TokenHash, refreshTokenValue))
        {
            return Result.Fail(new UnauthorizedError("Invalid refresh token."));
        }

        var now = DateTimeOffset.UtcNow;
        if (storedRefreshToken.ExpiresAt <= now)
        {
            storedRefreshToken.Status = RefreshTokenStatus.Revoked;
            storedRefreshToken.RevokedAt = now;
            storedRefreshToken.UpdatedAt = now;

            await _refreshTokenRepository.UpdateAsync(storedRefreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Fail(new UnauthorizedError("Invalid refresh token."));
        }

        var user = await _identityAuthService.FindByIdAsync(storedRefreshToken.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Fail(new UnauthorizedError("Invalid refresh token."));
        }

        storedRefreshToken.Status = RefreshTokenStatus.Revoked;
        storedRefreshToken.RevokedAt = now;
        storedRefreshToken.UpdatedAt = now;

        var refreshToken = _authTokenService.CreateRefreshToken(user);
        await _refreshTokenRepository.UpdateAsync(storedRefreshToken, cancellationToken);
        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = refreshToken.TokenId,
            UserId = refreshToken.UserId,
            TokenHash = refreshToken.TokenHash,
            ExpiresAt = refreshToken.ExpiresAt,
            Status = RefreshTokenStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _authTokenService.CreateAccessToken(user);

        return AuthenticationResultFactory.Create(user, accessToken, refreshToken);
    }
}
