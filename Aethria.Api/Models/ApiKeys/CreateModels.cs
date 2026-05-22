namespace Aethria.Api.Endpoints.ApiKeys;

/// <summary>
/// Request model for creating an API key.
/// </summary>
public sealed record CreateApiKeyRequest
{
    /// <summary>
    /// API key display name.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Number of days before the key expires.
    /// </summary>
    [Range(1, 365, ErrorMessage = "Expiration days must be between 1 and 365.")]
    public int ExpirationDays { get; init; }
}
