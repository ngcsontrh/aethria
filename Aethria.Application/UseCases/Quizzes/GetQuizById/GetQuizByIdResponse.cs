namespace Aethria.Application.UseCases.Quizzes.GetQuizById;

public sealed record GetQuizByIdResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ResourceId,
    string? ResourceName,
    int QuestionCount,
    int CurrentVersionNumber,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
