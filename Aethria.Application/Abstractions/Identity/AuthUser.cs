namespace Aethria.Application.Abstractions.Identity;

public sealed record AuthUser(
    Guid UserId,
    string Email);
