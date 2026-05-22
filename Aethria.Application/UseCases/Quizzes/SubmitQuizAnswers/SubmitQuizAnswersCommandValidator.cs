namespace Aethria.Application.UseCases.Quizzes.SubmitQuizAnswers;

internal sealed class SubmitQuizAnswersCommandValidator : AbstractValidator<SubmitQuizAnswersCommand>
{
    public SubmitQuizAnswersCommandValidator()
    {
        RuleFor(command => command.QuizId)
            .NotEmpty()
            .WithMessage("QuizId is required.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(command => command.QuizVersionId)
            .NotEmpty()
            .WithMessage("QuizVersionId is required.");

        RuleFor(command => command.Answers)
            .NotNull()
            .WithMessage("Answers are required.")
            .NotEmpty()
            .WithMessage("Answers are required.");

        RuleForEach(command => command.Answers)
            .ChildRules(answer =>
            {
                answer.RuleFor(item => item.QuestionSnapshotId)
                    .NotEmpty()
                    .WithMessage("QuestionSnapshotId is required.");

                answer.RuleFor(item => item.SelectedOptionId)
                    .NotEmpty()
                    .WithMessage("SelectedOptionId is required.");
            });
    }
}
