namespace Aethria.Application.UseCases.Auth.LoginWithEmail;

public sealed record LoginWithEmailCommand(
    string Email,
    string Password) : IRequest<LoginWithEmailCommand, ValueTask<Result<AuthenticationResult>>>;
