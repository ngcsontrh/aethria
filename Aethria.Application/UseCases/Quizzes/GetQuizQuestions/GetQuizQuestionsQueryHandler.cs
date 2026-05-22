namespace Aethria.Application.UseCases.Quizzes.GetQuizQuestions;

public class GetQuizQuestionsQueryHandler : IRequestHandler<GetQuizQuestionsQuery, ValueTask<Result<GetQuizQuestionsResponse>>>
{
    private readonly IQuizRepository _quizRepository;

    public GetQuizQuestionsQueryHandler(
        IQuizRepository quizRepository)
    {
        _quizRepository = quizRepository;
    }

    public async ValueTask<Result<GetQuizQuestionsResponse>> Handle(GetQuizQuestionsQuery request, CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetByIdWithCurrentVersionAsync(request.QuizId, cancellationToken);
        if (quiz == null)
        {
            return Result.Fail(new NotFoundError($"Quiz with ID {request.QuizId} not found."));
        }

        if (quiz.UserId != request.UserId)
        {
            return Result.Fail(new NotFoundError($"Quiz with ID {request.QuizId} not found."));
        }

        if (quiz.CurrentVersionNumber == 0)
        {
            return Result.Ok(new GetQuizQuestionsResponse(null, []));
        }

        var currentVersion = quiz.Versions.FirstOrDefault(v => v.VersionNumber == quiz.CurrentVersionNumber);
        if (currentVersion == null)
        {
            return Result.Ok(new GetQuizQuestionsResponse(null, []));
        }

        var questions = currentVersion.QuestionSnapshots
            .OrderBy(qs => qs.OrderIndex)
            .Select(qs => new QuizQuestionResponse(
                qs.Id,
                qs.Text,
                qs.OrderIndex,
                [.. qs.Options
                    .OrderBy(o => o.OrderIndex)
                    .Select(o => new QuestionOptionResponse(o.Id, o.Text, o.OrderIndex))]))
            .ToList();

        return Result.Ok(new GetQuizQuestionsResponse(currentVersion.Id, questions));
    }
}
