namespace Aethria.Application.UseCases.Quizzes.SubmitQuizAnswers;

public sealed record SubmitQuizAnswersCommand(
    Guid QuizId,
    Guid UserId,
    Guid QuizVersionId,
    List<SubmitAnswerItem> Answers) : IRequest<SubmitQuizAnswersCommand, ValueTask<Result<SubmitQuizAnswersResponse>>>;

public sealed record SubmitAnswerItem(
    Guid QuestionSnapshotId,
    Guid SelectedOptionId);
