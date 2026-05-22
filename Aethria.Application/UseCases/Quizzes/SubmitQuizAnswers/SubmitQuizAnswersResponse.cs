namespace Aethria.Application.UseCases.Quizzes.SubmitQuizAnswers;

public sealed record SubmitQuizAnswersResponse(
    Guid SubmissionId,
    int Score,
    int TotalQuestions,
    bool IsPassed,
    IReadOnlyList<AnswerResultResponse> AnswerResults);

public sealed record AnswerResultResponse(
    Guid QuestionSnapshotId,
    Guid SelectedOptionId,
    Guid CorrectOptionId,
    bool IsCorrect,
    string Explanation);
