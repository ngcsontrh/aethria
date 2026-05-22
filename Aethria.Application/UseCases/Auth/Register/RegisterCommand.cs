namespace Aethria.Application.UseCases.Auth.Register;

public sealed record RegisterCommand(
    string Email,
    string Password) : IRequest<RegisterCommand, ValueTask<Result<AuthenticationResult>>>;
