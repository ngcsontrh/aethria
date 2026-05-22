namespace Aethria.Application.UseCases.Quizzes.SubmitQuizAnswers;

public sealed class SubmitQuizAnswersCommandHandler : IRequestHandler<SubmitQuizAnswersCommand, ValueTask<Result<SubmitQuizAnswersResponse>>>
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitQuizAnswersCommandHandler(
        IQuizRepository quizRepository,
        IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<SubmitQuizAnswersResponse>> Handle(
        SubmitQuizAnswersCommand command,
        CancellationToken cancellationToken)
    {
        var quiz = await _quizRepository.GetByIdAsync(command.QuizId, cancellationToken);

        if (quiz is null)
        {
            return Result.Fail(new NotFoundError($"Quiz with ID {command.QuizId} not found."));
        }

        if (quiz.UserId != command.UserId)
        {
            return Result.Fail(new NotFoundError($"Quiz with ID {command.QuizId} not found."));
        }

        var quizVersion = await _quizRepository.GetVersionByIdWithSnapshotsAsync(command.QuizVersionId, cancellationToken);
        if (quizVersion is null || quizVersion.QuizId != command.QuizId)
        {
            return Result.Fail(new NotFoundError($"Quiz version with ID {command.QuizVersionId} not found."));
        }

        var questionSnapshots = quizVersion.QuestionSnapshots
            .OrderBy(qs => qs.OrderIndex)
            .ToList();
        if (questionSnapshots.Count == 0)
        {
            return Result.Fail(new ValidationError("Quiz has no questions to submit against."));
        }

        var questionSnapshotIds = questionSnapshots.Select(qs => qs.Id).ToHashSet();

        var duplicateAnswer = command.Answers
            .GroupBy(a => a.QuestionSnapshotId)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateAnswer is not null)
        {
            return Result.Fail(new ValidationError(
                $"Duplicate answer for QuestionSnapshotId: {duplicateAnswer.Key}."));
        }

        foreach (var answer in command.Answers)
        {
            if (!questionSnapshotIds.Contains(answer.QuestionSnapshotId))
            {
                return Result.Fail(new ValidationError(
                    $"Invalid QuestionSnapshotId: {answer.QuestionSnapshotId} does not belong to the submitted quiz version."));
            }
        }

        if (command.Answers.Count != questionSnapshots.Count)
        {
            return Result.Fail(new ValidationError("All quiz questions must be answered exactly once."));
        }

        foreach (var answer in command.Answers)
        {
            var questionSnapshot = questionSnapshots.First(qs => qs.Id == answer.QuestionSnapshotId);
            var optionIds = questionSnapshot.Options.Select(o => o.Id).ToHashSet();

            if (!optionIds.Contains(answer.SelectedOptionId))
            {
                return Result.Fail(new ValidationError(
                    $"Invalid SelectedOptionId: {answer.SelectedOptionId} does not belong to question {answer.QuestionSnapshotId}."));
            }
        }

        var score = 0;
        var answerResults = new List<AnswerResultResponse>();
        var answersByQuestionId = command.Answers.ToDictionary(a => a.QuestionSnapshotId);

        foreach (var questionSnapshot in questionSnapshots)
        {
            var answer = answersByQuestionId[questionSnapshot.Id];
            var isCorrect = answer.SelectedOptionId == questionSnapshot.CorrectOptionId;

            if (isCorrect)
            {
                score++;
            }

            answerResults.Add(new AnswerResultResponse(
                answer.QuestionSnapshotId,
                answer.SelectedOptionId,
                questionSnapshot.CorrectOptionId,
                isCorrect,
                questionSnapshot.Explanation));
        }

        var totalQuestions = questionSnapshots.Count;
        var isPassed = (double)score / totalQuestions >= 0.7;

        var now = DateTimeOffset.UtcNow;
        var submission = new QuizSubmission
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            QuizId = command.QuizId,
            QuizVersionId = quizVersion.Id,
            Score = score,
            TotalQuestions = totalQuestions,
            IsPassed = isPassed,
            CreatedAt = now,
            UpdatedAt = now,
            Answers = [.. command.Answers.Select(a =>
            {
                var questionSnapshot = questionSnapshots.First(qs => qs.Id == a.QuestionSnapshotId);
                return new SubmissionAnswer
                {
                    Id = Guid.NewGuid(),
                    QuestionSnapshotId = a.QuestionSnapshotId,
                    SelectedOptionId = a.SelectedOptionId,
                    IsCorrect = a.SelectedOptionId == questionSnapshot.CorrectOptionId,
                    CreatedAt = now,
                    UpdatedAt = now
                };
            })]
        };

        await _quizRepository.AddSubmissionAsync(submission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new SubmitQuizAnswersResponse(
            submission.Id,
            score,
            totalQuestions,
            isPassed,
            answerResults);

        return Result.Ok(response);
    }
}
