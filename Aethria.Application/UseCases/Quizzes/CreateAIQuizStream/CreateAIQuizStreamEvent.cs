namespace Aethria.Application.UseCases.Quizzes.CreateAIQuizStream;

public sealed record CreateAIQuizStreamEvent(
    string Status,
    string Message,
    Guid? QuizId = null);
