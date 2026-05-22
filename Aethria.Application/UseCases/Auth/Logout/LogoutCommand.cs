namespace Aethria.Application.UseCases.Auth.Logout;

public sealed record LogoutCommand(
    string? RefreshToken) : IRequest<LogoutCommand, ValueTask<Result>>;
