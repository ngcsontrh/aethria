namespace Aethria.Application.UseCases.Quizzes.UpdateQuiz;

public sealed record UpdateQuizCommand(
    Guid QuizId,
    Guid UserId,
    string? Name,
    string? Description,
    List<UpdateQuestionItem>? Questions) : IRequest<UpdateQuizCommand, ValueTask<Result<UpdateQuizResponse>>>;

public sealed record UpdateQuestionItem(
    string Text,
    string Explanation,
    int OrderIndex,
    List<UpdateOptionItem> Options,
    int CorrectOptionIndex);

public sealed record UpdateOptionItem(
    string Text,
    int OrderIndex);
