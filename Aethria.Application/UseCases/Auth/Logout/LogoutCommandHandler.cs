namespace Aethria.Application.UseCases.Auth.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, ValueTask<Result>>
{
    private readonly IAuthTokenService _authTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(
        IAuthTokenService authTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _authTokenService = authTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(command.RefreshToken)
            && _authTokenService.TryGetRefreshTokenId(command.RefreshToken, out var refreshTokenId))
        {
            var storedRefreshToken = await _refreshTokenRepository.GetActiveByIdAsync(refreshTokenId, cancellationToken);
            if (storedRefreshToken is not null
                && _authTokenService.RefreshTokenMatches(storedRefreshToken.TokenHash, command.RefreshToken))
            {
                var now = DateTimeOffset.UtcNow;
                storedRefreshToken.Status = RefreshTokenStatus.Revoked;
                storedRefreshToken.RevokedAt = now;
                storedRefreshToken.UpdatedAt = now;

                await _refreshTokenRepository.UpdateAsync(storedRefreshToken, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        return Result.Ok();
    }
}
