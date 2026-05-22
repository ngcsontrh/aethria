namespace Aethria.Application.UseCases.Auth.ChangePassword;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : IRequest<ChangePasswordCommand, ValueTask<Result>>;
