namespace Aethria.Application.UseCases.Quizzes.GetQuizSubmissionHistory;

public sealed record QuizSubmissionHistoryItemResponse(
    Guid SubmissionId,
    Guid QuizVersionId,
    int VersionNumber,
    int Score,
    int TotalQuestions,
    bool IsPassed,
    DateTimeOffset SubmittedAt);
