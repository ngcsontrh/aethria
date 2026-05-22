namespace Aethria.Application.UseCases.Quizzes.DeleteQuiz;

internal sealed class DeleteQuizCommandValidator : AbstractValidator<DeleteQuizCommand>
{
    public DeleteQuizCommandValidator()
    {
        RuleFor(command => command.QuizId)
            .NotEmpty()
            .WithMessage("QuizId is required.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
