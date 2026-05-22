namespace Aethria.Application.UseCases.Auth.ChangePassword;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ValueTask<Result>>
{
    private readonly IIdentityAuthService _identityAuthService;

    public ChangePasswordCommandHandler(
        IIdentityAuthService identityAuthService)
    {
        _identityAuthService = identityAuthService;
    }

    public async ValueTask<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityAuthService.ChangePasswordAsync(
            command.UserId,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken);

        if (!result.Succeeded)
        {
            return Result.Fail(new UnauthorizedError("Current password is incorrect."));
        }

        return Result.Ok();
    }
}
