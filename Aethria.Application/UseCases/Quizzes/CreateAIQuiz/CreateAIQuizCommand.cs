namespace Aethria.Application.UseCases.Quizzes.CreateAIQuiz;

public sealed record CreateAIQuizCommand(
    string Name,
    string? Description,
    Guid ResourceId,
    string? Prompt,
    int NumberOfQuestions,
    Guid UserId) : IRequest<CreateAIQuizCommand, ValueTask<Result<Guid>>>;
