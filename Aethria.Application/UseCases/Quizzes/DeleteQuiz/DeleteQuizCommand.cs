namespace Aethria.Application.UseCases.Quizzes.DeleteQuiz;

public sealed record DeleteQuizCommand(Guid QuizId, Guid UserId) : IRequest<DeleteQuizCommand, ValueTask<Result>>;
