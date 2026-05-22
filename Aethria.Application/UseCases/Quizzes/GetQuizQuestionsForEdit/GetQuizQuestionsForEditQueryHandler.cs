namespace Aethria.Application.UseCases.Quizzes.GetQuizQuestionsForEdit;

public class GetQuizQuestionsForEditQueryHandler : IRequestHandler<GetQuizQuestionsForEditQuery, ValueTask<Result<GetQuizQuestionsForEditResponse>>>
{
    private readonly IQuizRepository _quizRepository;

    public GetQuizQuestionsForEditQueryHandler(
        IQuizRepository quizRepository)
    {
        _quizRepository = quizRepository;
    }

    public async ValueTask<Result<GetQuizQuestionsForEditResponse>> Handle(
        GetQuizQuestionsForEditQuery request,
        CancellationToken cancellationToken)
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
            return Result.Ok(new GetQuizQuestionsForEditResponse(null, []));
        }

        var currentVersion = quiz.Versions.FirstOrDefault(v => v.VersionNumber == quiz.CurrentVersionNumber);
        if (currentVersion == null)
        {
            return Result.Ok(new GetQuizQuestionsForEditResponse(null, []));
        }

        var questions = new List<QuizQuestionForEditResponse>();
        foreach (var snapshot in currentVersion.QuestionSnapshots.OrderBy(qs => qs.OrderIndex))
        {
            var options = snapshot.Options
                .OrderBy(o => o.OrderIndex)
                .ToList();

            var correctOptionIndex = options.FindIndex(o => o.Id == snapshot.CorrectOptionId);
            if (correctOptionIndex < 0)
            {
                return Result.Fail(new ValidationError("Quiz question snapshot has an invalid correct option."));
            }

            questions.Add(new QuizQuestionForEditResponse(
                snapshot.Id,
                snapshot.Text,
                snapshot.Explanation,
                snapshot.OrderIndex,
                correctOptionIndex,
                [.. options.Select(o => new QuestionOptionForEditResponse(o.Id, o.Text, o.OrderIndex))]));
        }

        return Result.Ok(new GetQuizQuestionsForEditResponse(currentVersion.Id, questions));
    }
}
