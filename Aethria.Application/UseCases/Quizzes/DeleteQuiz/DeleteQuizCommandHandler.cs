namespace Aethria.Application.UseCases.Quizzes.DeleteQuiz;

public sealed class DeleteQuizCommandHandler : IRequestHandler<DeleteQuizCommand, ValueTask<Result>>
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteQuizCommandHandler(
        IQuizRepository quizRepository,
        IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(
        DeleteQuizCommand command,
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

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _quizRepository.DeleteSubmissionsByQuizIdAsync(quiz.Id, cancellationToken);
            await _quizRepository.DeleteAsync(quiz, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return Result.Ok();
    }
}
