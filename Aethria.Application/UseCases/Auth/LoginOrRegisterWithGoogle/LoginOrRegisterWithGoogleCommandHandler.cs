namespace Aethria.Application.UseCases.Auth.LoginOrRegisterWithGoogle;

public sealed class LoginOrRegisterWithGoogleCommandHandler : IRequestHandler<LoginOrRegisterWithGoogleCommand, ValueTask<Result<AuthenticationResult>>>
{
    private readonly IIdentityAuthService _identityAuthService;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IAuthTokenService _authTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginOrRegisterWithGoogleCommandHandler(
        IIdentityAuthService identityAuthService,
        IGoogleTokenValidator googleTokenValidator,
        IAuthTokenService authTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _identityAuthService = identityAuthService;
        _googleTokenValidator = googleTokenValidator;
        _authTokenService = authTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    private const string GoogleLoginProvider = "Google";

    public async ValueTask<Result<AuthenticationResult>> Handle(LoginOrRegisterWithGoogleCommand command, CancellationToken cancellationToken)
    {
        var googleUser = await _googleTokenValidator.ValidateAsync(command.IdToken, cancellationToken);
        if (googleUser is null || !googleUser.EmailVerified)
        {
            return Result.Fail(new UnauthorizedError("Invalid Google token."));
        }

        var user = await _identityAuthService.FindByLoginAsync(GoogleLoginProvider, googleUser.Subject, cancellationToken);
        if (user is null)
        {
            user = await FindOrCreateUserAsync(googleUser, cancellationToken);
            if (user is null)
            {
                return Result.Fail(new ValidationError("Google sign-in failed."));
            }

            var linkResult = await _identityAuthService.AddLoginAsync(
                user.UserId,
                new ExternalLoginInfo(GoogleLoginProvider, googleUser.Subject, GoogleLoginProvider),
                cancellationToken);

            if (!linkResult.Succeeded)
            {
                return Result.Fail(new ValidationError(BuildErrorMessage(linkResult.Errors, "Google sign-in failed.")));
            }
        }

        return await AuthenticationResultFactory.CreateAsync(
            user,
            _authTokenService,
            _refreshTokenRepository,
            _unitOfWork,
            cancellationToken);
    }

    private async Task<AuthUser?> FindOrCreateUserAsync(GoogleUserInfo googleUser, CancellationToken cancellationToken)
    {
        var user = await _identityAuthService.FindByEmailAsync(googleUser.Email, cancellationToken);
        if (user is not null)
        {
            return user;
        }

        var createResult = await _identityAuthService.CreateUserAsync(
            googleUser.Email,
            password: null,
            emailConfirmed: true,
            cancellationToken);

        return createResult.Succeeded ? createResult.User : null;
    }

    private static string BuildErrorMessage(IReadOnlyList<string> errors, string fallback)
    {
        return errors.Count == 0 ? fallback : string.Join(" ", errors);
    }
}
