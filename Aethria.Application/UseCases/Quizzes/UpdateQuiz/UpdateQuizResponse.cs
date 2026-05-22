namespace Aethria.Application.UseCases.Quizzes.UpdateQuiz;

public sealed record UpdateQuizResponse(
    Guid QuizId,
    string Name,
    string? Description,
    int CurrentVersionNumber);
