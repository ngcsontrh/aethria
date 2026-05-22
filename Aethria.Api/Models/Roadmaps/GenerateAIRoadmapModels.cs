namespace Aethria.Api.Endpoints.Roadmaps;

/// <summary>
/// AI roadmap generation request.
/// </summary>
public sealed record GenerateAIRoadmapRequest
{
    /// <summary>
    /// Roadmap display name.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters.")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Optional roadmap description.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description must be 1000 characters or fewer.")]
    public string? Description { get; init; }

    /// <summary>
    /// Source resource identifier used to generate the roadmap.
    /// </summary>
    [Required(ErrorMessage = "Resource id is required.")]
    public Guid ResourceId { get; init; }

    /// <summary>
    /// Optional generation prompt.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Prompt must be 2000 characters or fewer.")]
    public string? Prompt { get; init; }
}
