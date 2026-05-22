namespace Aethria.Application.UseCases.Auth.LoginWithEmail;

public sealed class LoginWithEmailCommandHandler : IRequestHandler<LoginWithEmailCommand, ValueTask<Result<AuthenticationResult>>>
{
    private readonly IIdentityAuthService _identityAuthService;
    private readonly IAuthTokenService _authTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginWithEmailCommandHandler(
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

    public async ValueTask<Result<AuthenticationResult>> Handle(LoginWithEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await _identityAuthService.FindByEmailAsync(command.Email.Trim(), cancellationToken);
        if (user is null)
        {
            return Result.Fail(new UnauthorizedError("Invalid email or password."));
        }

        var passwordIsValid = await _identityAuthService.CheckPasswordAsync(user.UserId, command.Password, cancellationToken);
        if (!passwordIsValid)
        {
            return Result.Fail(new UnauthorizedError("Invalid email or password."));
        }

        return await AuthenticationResultFactory.CreateAsync(
            user,
            _authTokenService,
            _refreshTokenRepository,
            _unitOfWork,
            cancellationToken);
    }
}
