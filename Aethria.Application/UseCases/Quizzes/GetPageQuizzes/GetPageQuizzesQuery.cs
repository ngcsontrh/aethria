namespace Aethria.Application.UseCases.Quizzes.GetPageQuizzes;

public sealed record GetPageQuizzesQuery(Guid UserId, int PageNumber, int PageSize) : IRequest<GetPageQuizzesQuery, ValueTask<Result<PagedResponse<QuizPageItemResponse>>>>;
