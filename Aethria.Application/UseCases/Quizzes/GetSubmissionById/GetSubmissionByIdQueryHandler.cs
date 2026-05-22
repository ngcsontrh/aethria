namespace Aethria.Application.UseCases.Quizzes.GetSubmissionById;

public class GetSubmissionByIdQueryHandler : IRequestHandler<GetSubmissionByIdQuery, ValueTask<Result<GetSubmissionByIdResponse>>>
{
    private readonly IQuizRepository _quizRepository;

    public GetSubmissionByIdQueryHandler(
        IQuizRepository quizRepository)
    {
        _quizRepository = quizRepository;
    }

    public async ValueTask<Result<GetSubmissionByIdResponse>> Handle(GetSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var submission = await _quizRepository.GetSubmissionByIdAsync(request.SubmissionId, cancellationToken);
        if (submission == null)
        {
            return Result.Fail(new NotFoundError($"Submission with ID {request.SubmissionId} not found."));
        }

        if (submission.QuizId != request.QuizId)
        {
            return Result.Fail(new NotFoundError($"Submission with ID {request.SubmissionId} not found."));
        }

        if (submission.UserId != request.UserId)
        {
            return Result.Fail(new NotFoundError($"Submission with ID {request.SubmissionId} not found."));
        }

        var quizVersion = await _quizRepository.GetVersionByIdWithSnapshotsAsync(submission.QuizVersionId, cancellationToken);

        var questionSnapshots = quizVersion?.QuestionSnapshots
            .OrderBy(qs => qs.OrderIndex)
            .ToList() ?? [];

        var answersByQuestionId = submission.Answers
            .GroupBy(a => a.QuestionSnapshotId)
            .ToDictionary(g => g.Key, g => g.First());

        var questions = new List<SubmissionQuestionResponse>();

        foreach (var snapshot in questionSnapshots)
        {
            var options = snapshot.Options
                .OrderBy(o => o.OrderIndex)
                .Select(o => new SubmissionOptionResponse(o.Id, o.Text, o.OrderIndex))
                .ToList();

            if (answersByQuestionId.TryGetValue(snapshot.Id, out var answer))
            {
                questions.Add(new SubmissionQuestionResponse(
                    snapshot.Id,
                    snapshot.Text,
                    snapshot.Explanation,
                    snapshot.OrderIndex,
                    answer.SelectedOptionId,
                    snapshot.CorrectOptionId,
                    answer.IsCorrect,
                    options));
            }
            else
            {
                questions.Add(new SubmissionQuestionResponse(
                    snapshot.Id,
                    snapshot.Text,
                    snapshot.Explanation,
                    snapshot.OrderIndex,
                    Guid.Empty,
                    snapshot.CorrectOptionId,
                    false,
                    options));
            }
        }

        var response = new GetSubmissionByIdResponse(
            submission.Id,
            submission.Score,
            submission.TotalQuestions,
            submission.IsPassed,
            quizVersion?.VersionNumber ?? 0,
            submission.CreatedAt,
            questions);

        return Result.Ok(response);
    }
}
