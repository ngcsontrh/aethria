namespace Aethria.Application.UseCases.Auth.RefreshAccessToken;

public sealed record RefreshAccessTokenCommand(
    string? RefreshToken) : IRequest<RefreshAccessTokenCommand, ValueTask<Result<AuthenticationResult>>>;
