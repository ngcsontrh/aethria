namespace Aethria.Api.Endpoints.Auth;

/// <summary>
/// Password change request.
/// </summary>
public sealed record ChangePasswordRequest
{
    /// <summary>
    /// Current password for verification.
    /// </summary>
    [Required(ErrorMessage = "Current password is required.")]
    [StringLength(128, MinimumLength = 1, ErrorMessage = "Current password must be between 1 and 128 characters.")]
    public string CurrentPassword { get; init; } = null!;

    /// <summary>
    /// New password to store for the account.
    /// </summary>
    [Required(ErrorMessage = "New password is required.")]
    [StringLength(128, MinimumLength = 6, ErrorMessage = "New password must be between 6 and 128 characters.")]
    public string NewPassword { get; init; } = null!;
}
