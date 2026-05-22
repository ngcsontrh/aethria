namespace Aethria.Application.UseCases.Quizzes.GetPageQuizzes;

public class GetPageQuizzesQueryHandler : IRequestHandler<GetPageQuizzesQuery, ValueTask<Result<PagedResponse<QuizPageItemResponse>>>>
{
    private readonly IQuizRepository _quizRepository;

    public GetPageQuizzesQueryHandler(
        IQuizRepository quizRepository)
    {
        _quizRepository = quizRepository;
    }

    public async ValueTask<Result<PagedResponse<QuizPageItemResponse>>> Handle(GetPageQuizzesQuery request, CancellationToken cancellationToken)
    {
        var (quizzes, totalCount) = await _quizRepository.GetPageByUserIdAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var items = quizzes.Select(q => new QuizPageItemResponse(
            q.Id,
            q.Name,
            q.Description,
            q.ResourceId,
            q.Questions.Count,
            q.CreatedAt,
            q.UpdatedAt
        )).ToList();

        return Result.Ok(new PagedResponse<QuizPageItemResponse>(items, totalCount, request.PageNumber, request.PageSize));
    }
}
