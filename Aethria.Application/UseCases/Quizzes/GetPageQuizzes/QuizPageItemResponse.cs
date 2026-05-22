namespace Aethria.Application.UseCases.Quizzes.GetPageQuizzes;

public sealed record QuizPageItemResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ResourceId,
    int QuestionCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
