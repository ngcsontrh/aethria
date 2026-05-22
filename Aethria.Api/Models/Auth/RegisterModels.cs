namespace Aethria.Api.Endpoints.Auth;

/// <summary>
/// Email/password registration request.
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>
    /// Email address used for authentication.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [StringLength(256, ErrorMessage = "Email must be 256 characters or fewer.")]
    public string Email { get; init; } = null!;

    /// <summary>
    /// Plain-text password submitted by the user.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(128, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 128 characters.")]
    public string Password { get; init; } = null!;
}
