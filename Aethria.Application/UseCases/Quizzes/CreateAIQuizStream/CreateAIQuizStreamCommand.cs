namespace Aethria.Application.UseCases.Quizzes.CreateAIQuizStream;

public sealed record CreateAIQuizStreamCommand(
    string Name,
    string? Description,
    Guid ResourceId,
    string? Prompt,
    int NumberOfQuestions,
    Guid UserId) : IStreamRequest<CreateAIQuizStreamCommand, Result<CreateAIQuizStreamEvent>>;
