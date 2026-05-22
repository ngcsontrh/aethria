namespace Aethria.Api.Endpoints.Auth;

/// <summary>
/// Google authentication request.
/// </summary>
public sealed record GoogleRequest
{
    /// <summary>
    /// Google ID token returned by the client.
    /// </summary>
    [Required(ErrorMessage = "Google ID token is required.")]
    [StringLength(8192, MinimumLength = 1, ErrorMessage = "Google ID token must be between 1 and 8192 characters.")]
    public string IdToken { get; init; } = null!;
}
