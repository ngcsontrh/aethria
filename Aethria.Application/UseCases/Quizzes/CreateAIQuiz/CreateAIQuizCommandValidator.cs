namespace Aethria.Application.UseCases.Quizzes.CreateAIQuiz;

internal sealed class CreateAIQuizCommandValidator : AbstractValidator<CreateAIQuizCommand>
{
    public CreateAIQuizCommandValidator()
    {
        RuleFor(command => command.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Quiz name is required.");

        RuleFor(command => command.ResourceId)
            .NotEmpty()
            .WithMessage("ResourceId is required for AI quiz generation.");

        RuleFor(command => command.NumberOfQuestions)
            .InclusiveBetween(1, 50)
            .WithMessage("Number of questions must be between 1 and 50.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
