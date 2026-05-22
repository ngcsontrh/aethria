namespace Aethria.Api.Endpoints.Resources;

/// <summary>
/// Resource creation request.
/// </summary>
public sealed class CreateResourceRequest
{
    /// <summary>
    /// Resource display name.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters.")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Optional resource description.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description must be 1000 characters or fewer.")]
    public string? Description { get; init; }

    /// <summary>
    /// Uploaded resource file.
    /// </summary>
    [Required(ErrorMessage = "File is required.")]
    public IFormFile File { get; init; } = null!;
}
