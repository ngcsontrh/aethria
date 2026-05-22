namespace Aethria.Application.UseCases.Quizzes.GetQuizSubmissionHistory;

public class GetQuizSubmissionHistoryQueryHandler : IRequestHandler<GetQuizSubmissionHistoryQuery, ValueTask<Result<PagedResponse<QuizSubmissionHistoryItemResponse>>>>
{
    private readonly IQuizRepository _quizRepository;

    public GetQuizSubmissionHistoryQueryHandler(
        IQuizRepository quizRepository)
    {
        _quizRepository = quizRepository;
    }

    public async ValueTask<Result<PagedResponse<QuizSubmissionHistoryItemResponse>>> Handle(
        GetQuizSubmissionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetByIdAsync(request.QuizId, cancellationToken);
        if (quiz is null || quiz.UserId != request.UserId)
        {
            return Result.Fail(new NotFoundError($"Quiz with ID {request.QuizId} not found."));
        }

        var (submissions, totalCount) = await _quizRepository.GetSubmissionHistoryByQuizIdAsync(
            request.QuizId,
            request.UserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var items = submissions
            .Select(s => new QuizSubmissionHistoryItemResponse(
                s.SubmissionId,
                s.QuizVersionId,
                s.VersionNumber,
                s.Score,
                s.TotalQuestions,
                s.IsPassed,
                s.SubmittedAt))
            .ToList();

        return Result.Ok(new PagedResponse<QuizSubmissionHistoryItemResponse>(items, totalCount, request.PageNumber, request.PageSize));
    }
}
