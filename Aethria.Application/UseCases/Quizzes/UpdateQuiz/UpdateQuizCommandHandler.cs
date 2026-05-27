namespace Aethria.Application.UseCases.Quizzes.UpdateQuiz;

public sealed class UpdateQuizCommandHandler : IRequestHandler<UpdateQuizCommand, ValueTask<Result<UpdateQuizResponse>>>
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuizCommandHandler(
        IQuizRepository quizRepository,
        IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<UpdateQuizResponse>> Handle(
        UpdateQuizCommand command,
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

        if (command.Name is not null)
        {
            quiz.Name = command.Name;
        }

        if (command.Description is not null)
        {
            quiz.Description = command.Description;
        }

        quiz.UpdatedAt = DateTimeOffset.UtcNow;

        if (command.Questions is null)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var newQuestions = new List<QuizQuestion>();
            var now = DateTimeOffset.UtcNow;

            foreach (var questionItem in command.Questions)
            {
                var question = new QuizQuestion
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    Text = questionItem.Text,
                    Explanation = questionItem.Explanation,
                    OrderIndex = questionItem.OrderIndex,
                    CreatedAt = now,
                    UpdatedAt = now,
                    Options = []
                };

                foreach (var optionItem in questionItem.Options)
                {
                    question.Options.Add(new QuestionOption
                    {
                        Id = Guid.NewGuid(),
                        QuizQuestionId = question.Id,
                        Text = optionItem.Text,
                        OrderIndex = optionItem.OrderIndex,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }

                var optionsList = question.Options.ToList();
                question.CorrectOptionId = optionsList[questionItem.CorrectOptionIndex].Id;

                newQuestions.Add(question);
            }

            var newVersionNumber = quiz.CurrentVersionNumber + 1;
            var newVersionId = Guid.NewGuid();

            var questionSnapshots = new List<QuestionSnapshot>();
            foreach (var q in newQuestions)
            {
                var snapshot = new QuestionSnapshot
                {
                    Id = Guid.NewGuid(),
                    QuizVersionId = newVersionId,
                    OriginalQuestionId = q.Id,
                    Text = q.Text,
                    Explanation = q.Explanation,
                    OrderIndex = q.OrderIndex,
                    CreatedAt = now,
                    UpdatedAt = now,
                    Options = []
                };

                foreach (var option in q.Options)
                {
                    snapshot.Options.Add(new QuestionOptionSnapshot
                    {
                        Id = Guid.NewGuid(),
                        QuestionSnapshotId = snapshot.Id,
                        OriginalOptionId = option.Id,
                        Text = option.Text,
                        OrderIndex = option.OrderIndex,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }

                snapshot.CorrectOptionId = snapshot.Options.First(o => o.OriginalOptionId == q.CorrectOptionId).Id;
                questionSnapshots.Add(snapshot);
            }

            var newVersion = new QuizVersion
            {
                Id = newVersionId,
                QuizId = quiz.Id,
                VersionNumber = newVersionNumber,
                CreatedAt = now,
                UpdatedAt = now,
                QuestionSnapshots = questionSnapshots
            };

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                await _quizRepository.DeleteQuestionsByQuizIdAsync(quiz.Id, ct);

                foreach (var q in newQuestions)
                {
                    quiz.Questions.Add(q);
                }

                quiz.Versions.Add(newVersion);
                quiz.CurrentVersionNumber = newVersionNumber;

                await _unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);
        }

        var response = new UpdateQuizResponse(
            quiz.Id,
            quiz.Name,
            quiz.Description,
            quiz.CurrentVersionNumber);

        return Result.Ok(response);
    }
}
