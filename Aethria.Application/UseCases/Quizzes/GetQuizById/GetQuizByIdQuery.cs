namespace Aethria.Application.UseCases.Quizzes.GetQuizById;

public sealed record GetQuizByIdQuery(Guid QuizId, Guid UserId) : IRequest<GetQuizByIdQuery, ValueTask<Result<GetQuizByIdResponse>>>;
