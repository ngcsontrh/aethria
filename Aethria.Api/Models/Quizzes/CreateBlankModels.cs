namespace Aethria.Api.Endpoints.Quizzes;

/// <summary>
/// Blank quiz creation request.
/// </summary>
public sealed record CreateBlankQuizRequest
{
    /// <summary>
    /// Quiz display name.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters.")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Optional quiz description.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description must be 1000 characters or fewer.")]
    public string? Description { get; init; }

    /// <summary>
    /// Optional source resource identifier.
    /// </summary>
    public Guid? ResourceId { get; init; }
}
