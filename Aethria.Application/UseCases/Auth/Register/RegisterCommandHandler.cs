namespace Aethria.Application.UseCases.Auth.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, ValueTask<Result<AuthenticationResult>>>
{
    private readonly IIdentityAuthService _identityAuthService;
    private readonly IAuthTokenService _authTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
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

    public async ValueTask<Result<AuthenticationResult>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityAuthService.CreateUserAsync(
            command.Email.Trim(),
            command.Password,
            emailConfirmed: false,
            cancellationToken);

        if (!result.Succeeded || result.User is null)
        {
            return Result.Fail(new ValidationError(BuildErrorMessage(result.Errors, "Registration failed.")));
        }

        return await AuthenticationResultFactory.CreateAsync(
            result.User,
            _authTokenService,
            _refreshTokenRepository,
            _unitOfWork,
            cancellationToken);
    }

    private static string BuildErrorMessage(IReadOnlyList<string> errors, string fallback)
    {
        return errors.Count == 0 ? fallback : string.Join(" ", errors);
    }
}
