namespace Aethria.Api.Endpoints.Resources;

/// <summary>
/// Resource update request.
/// </summary>
public sealed record UpdateResourceRequest
{
    /// <summary>
    /// Updated resource display name.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters.")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Updated resource description.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description must be 1000 characters or fewer.")]
    public string? Description { get; init; }
}
