namespace Aethria.Application.UseCases.Quizzes.GetQuizSubmissionHistory;

public sealed record GetQuizSubmissionHistoryQuery(
    Guid QuizId,
    Guid UserId,
    int PageNumber,
    int PageSize) : IRequest<GetQuizSubmissionHistoryQuery, ValueTask<Result<PagedResponse<QuizSubmissionHistoryItemResponse>>>>;
