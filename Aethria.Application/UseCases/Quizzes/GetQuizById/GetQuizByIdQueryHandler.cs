namespace Aethria.Application.UseCases.Quizzes.GetQuizById;

public class GetQuizByIdQueryHandler : IRequestHandler<GetQuizByIdQuery, ValueTask<Result<GetQuizByIdResponse>>>
{
    private readonly IQuizRepository _quizRepository;
    private readonly IResourceRepository _resourceRepository;

    public GetQuizByIdQueryHandler(
        IQuizRepository quizRepository,
        IResourceRepository resourceRepository)
    {
        _quizRepository = quizRepository;
        _resourceRepository = resourceRepository;
    }

    public async ValueTask<Result<GetQuizByIdResponse>> Handle(GetQuizByIdQuery request, CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetByIdWithQuestionsAsync(request.QuizId, cancellationToken);
        if (quiz == null)
        {
            return Result.Fail(new NotFoundError($"Quiz with ID {request.QuizId} not found."));
        }

        if (quiz.UserId != request.UserId)
        {
            return Result.Fail(new NotFoundError($"Quiz with ID {request.QuizId} not found."));
        }

        string? resourceName = null;
        if (quiz.ResourceId != null)
        {
            var resource = await _resourceRepository.GetByIdAsync(quiz.ResourceId.Value, cancellationToken);
            resourceName = resource?.Name;
        }

        return Result.Ok(new GetQuizByIdResponse(
            quiz.Id,
            quiz.Name,
            quiz.Description,
            quiz.ResourceId,
            resourceName,
            quiz.Questions.Count,
            quiz.CurrentVersionNumber,
            quiz.CreatedAt,
            quiz.UpdatedAt));
    }
}
