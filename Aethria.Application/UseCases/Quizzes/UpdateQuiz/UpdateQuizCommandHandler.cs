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
        var quiz = await _quizRepository.GetByIdWithQuestionsAndVersionsAsync(command.QuizId, cancellationToken);

        if (quiz is null)
        {
            return Result.Fail(new NotFoundError($"Quiz with ID {command.QuizId} not found."));
        }

        if (quiz.UserId != command.UserId)
        {
            return Result.Fail(new NotFoundError($"Quiz with ID {command.QuizId} not found."));
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            if (command.Name is not null)
            {
                quiz.Name = command.Name;
            }

            if (command.Description is not null)
            {
                quiz.Description = command.Description;
            }

            quiz.UpdatedAt = DateTimeOffset.UtcNow;

            if (command.Questions is not null)
            {
                quiz.Questions.Clear();

                var newQuestions = new List<QuizQuestion>();
                foreach (var questionItem in command.Questions)
                {
                    var question = new QuizQuestion
                    {
                        Id = Guid.NewGuid(),
                        QuizId = quiz.Id,
                        Text = questionItem.Text,
                        Explanation = questionItem.Explanation,
                        OrderIndex = questionItem.OrderIndex,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        Options = [.. questionItem.Options.Select(o => new QuestionOption
                        {
                            Id = Guid.NewGuid(),
                            Text = o.Text,
                            OrderIndex = o.OrderIndex,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow
                        })]
                    };

                    var optionsList = question.Options.ToList();
                    question.CorrectOptionId = optionsList[questionItem.CorrectOptionIndex].Id;

                    newQuestions.Add(question);
                }

                foreach (var q in newQuestions)
                {
                    quiz.Questions.Add(q);
                }

                var newVersionNumber = quiz.CurrentVersionNumber + 1;

                var questionSnapshots = new List<QuestionSnapshot>();
                foreach (var q in newQuestions)
                {
                    var snapshotOptions = q.Options.Select(o => new QuestionOptionSnapshot
                    {
                        Id = Guid.NewGuid(),
                        OriginalOptionId = o.Id,
                        Text = o.Text,
                        OrderIndex = o.OrderIndex,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    }).ToList();

                    questionSnapshots.Add(new QuestionSnapshot
                    {
                        Id = Guid.NewGuid(),
                        OriginalQuestionId = q.Id,
                        Text = q.Text,
                        Explanation = q.Explanation,
                        OrderIndex = q.OrderIndex,
                        CorrectOptionId = snapshotOptions.First(o => o.OriginalOptionId == q.CorrectOptionId).Id,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        Options = snapshotOptions
                    });
                }

                var newVersion = new QuizVersion
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    VersionNumber = newVersionNumber,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    QuestionSnapshots = questionSnapshots
                };

                quiz.Versions.Add(newVersion);
                quiz.CurrentVersionNumber = newVersionNumber;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        var response = new UpdateQuizResponse(
            quiz.Id,
            quiz.Name,
            quiz.Description,
            quiz.CurrentVersionNumber);

        return Result.Ok(response);
    }
}
