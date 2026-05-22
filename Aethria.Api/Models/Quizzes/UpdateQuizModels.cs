namespace Aethria.Api.Endpoints.Quizzes;

/// <summary>
/// Quiz update request.
/// </summary>
public sealed record UpdateQuizRequest
{
    /// <summary>
    /// Updated quiz display name.
    /// </summary>
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters.")]
    public string? Name { get; init; }

    /// <summary>
    /// Updated quiz description.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description must be 1000 characters or fewer.")]
    public string? Description { get; init; }

    /// <summary>
    /// Updated quiz questions.
    /// </summary>
    [MinLength(1, ErrorMessage = "Questions must contain at least one item.")]
    public List<UpdateQuestionRequest>? Questions { get; init; }
}

/// <summary>
/// Quiz question update request.
/// </summary>
public sealed record UpdateQuestionRequest
{
    /// <summary>
    /// Question text shown to learners.
    /// </summary>
    [Required(ErrorMessage = "Question text is required.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Question text must be between 1 and 2000 characters.")]
    public string Text { get; init; } = null!;

    /// <summary>
    /// Explanation shown after answering.
    /// </summary>
    [Required(ErrorMessage = "Explanation is required.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Explanation must be between 1 and 2000 characters.")]
    public string Explanation { get; init; } = null!;

    /// <summary>
    /// Question order within the quiz.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Order index must be zero or greater.")]
    public int OrderIndex { get; init; }

    /// <summary>
    /// Available answer options.
    /// </summary>
    [Required(ErrorMessage = "Options are required.")]
    [MinLength(2, ErrorMessage = "Options must contain at least two items.")]
    public List<UpdateOptionRequest> Options { get; init; } = [];

    /// <summary>
    /// Index of the correct answer option.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Correct option index must be zero or greater.")]
    public int CorrectOptionIndex { get; init; }
}

/// <summary>
/// Quiz option update request.
/// </summary>
public sealed record UpdateOptionRequest
{
    /// <summary>
    /// Option text shown to learners.
    /// </summary>
    [Required(ErrorMessage = "Option text is required.")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Option text must be between 1 and 1000 characters.")]
    public string Text { get; init; } = null!;

    /// <summary>
    /// Option order within the question.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Order index must be zero or greater.")]
    public int OrderIndex { get; init; }
}
