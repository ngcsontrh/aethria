namespace Aethria.Api.Endpoints.Quizzes;

/// <summary>
/// AI quiz creation request.
/// </summary>
public sealed record CreateAIQuizRequest
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
    /// Source resource identifier used to generate the quiz.
    /// </summary>
    [Required(ErrorMessage = "Resource id is required.")]
    public Guid ResourceId { get; init; }

    /// <summary>
    /// Optional generation prompt.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Prompt must be 2000 characters or fewer.")]
    public string? Prompt { get; init; }

    /// <summary>
    /// Number of questions to generate.
    /// </summary>
    [Required(ErrorMessage = "Number of questions is required.")]
    [Range(1, 50, ErrorMessage = "Number of questions must be between 1 and 50.")]
    public int NumberOfQuestions { get; init; }
}
