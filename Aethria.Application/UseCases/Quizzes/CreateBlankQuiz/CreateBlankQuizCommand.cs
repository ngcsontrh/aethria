namespace Aethria.Application.UseCases.Quizzes.CreateBlankQuiz;

public sealed record CreateBlankQuizCommand(
    string Name,
    string? Description,
    Guid? ResourceId,
    Guid UserId) : IRequest<CreateBlankQuizCommand, ValueTask<Result<Guid>>>;
