namespace Aethria.Application.UseCases.Quizzes.GetSubmissionById;

public sealed record GetSubmissionByIdResponse(
    Guid SubmissionId,
    int Score,
    int TotalQuestions,
    bool IsPassed,
    int VersionNumber,
    DateTimeOffset SubmittedAt,
    IReadOnlyList<SubmissionQuestionResponse> Questions);

public sealed record SubmissionQuestionResponse(
    Guid QuestionSnapshotId,
    string Text,
    string Explanation,
    int OrderIndex,
    Guid SelectedOptionId,
    Guid CorrectOptionId,
    bool IsCorrect,
    IReadOnlyList<SubmissionOptionResponse> Options);

public sealed record SubmissionOptionResponse(
    Guid Id,
    string Text,
    int OrderIndex);
