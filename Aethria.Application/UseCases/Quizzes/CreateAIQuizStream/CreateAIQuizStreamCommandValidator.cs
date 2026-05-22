namespace Aethria.Application.UseCases.Quizzes.CreateAIQuizStream;

internal sealed class CreateAIQuizStreamCommandValidator : AbstractValidator<CreateAIQuizStreamCommand>
{
    public CreateAIQuizStreamCommandValidator()
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
