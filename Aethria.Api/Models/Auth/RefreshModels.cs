namespace Aethria.Api.Endpoints.Auth;

/// <summary>
/// Authentication response.
/// </summary>
public sealed record AuthResponse
{
    /// <summary>
    /// Authenticated user identifier.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Authenticated user's email address.
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// Access token used for API authorization.
    /// </summary>
    public string AccessToken { get; init; } = null!;

    /// <summary>
    /// Expiration time of the access token.
    /// </summary>
    public DateTimeOffset AccessTokenExpiresAt { get; init; }
}

