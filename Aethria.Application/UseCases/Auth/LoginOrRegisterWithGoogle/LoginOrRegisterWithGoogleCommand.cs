namespace Aethria.Application.UseCases.Auth.LoginOrRegisterWithGoogle;

public sealed record LoginOrRegisterWithGoogleCommand(
    string IdToken) : IRequest<LoginOrRegisterWithGoogleCommand, ValueTask<Result<AuthenticationResult>>>;
