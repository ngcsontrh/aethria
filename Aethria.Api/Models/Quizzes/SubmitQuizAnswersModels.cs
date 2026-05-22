namespace Aethria.Api.Endpoints.Quizzes;

/// <summary>
/// Quiz submission request.
/// </summary>
public sealed record SubmitQuizRequest
{
    /// <summary>
    /// Quiz version identifier being submitted.
    /// </summary>
    [Required(ErrorMessage = "Quiz version id is required.")]
    public Guid QuizVersionId { get; init; }

    /// <summary>
    /// Selected answers for quiz questions.
    /// </summary>
    [Required(ErrorMessage = "Answers are required.")]
    [MinLength(1, ErrorMessage = "At least one answer is required.")]
    public List<SubmitAnswerRequest> Answers { get; init; } = [];
}

/// <summary>
/// Submitted answer request.
/// </summary>
public sealed record SubmitAnswerRequest
{
    /// <summary>
    /// Question snapshot identifier being answered.
    /// </summary>
    [Required(ErrorMessage = "Question snapshot id is required.")]
    public Guid QuestionSnapshotId { get; init; }

    /// <summary>
    /// Selected option identifier.
    /// </summary>
    [Required(ErrorMessage = "Selected option id is required.")]
    public Guid SelectedOptionId { get; init; }
}
