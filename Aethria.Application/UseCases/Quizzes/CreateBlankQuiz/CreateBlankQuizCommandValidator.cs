namespace Aethria.Application.UseCases.Quizzes.CreateBlankQuiz;

internal sealed class CreateBlankQuizCommandValidator : AbstractValidator<CreateBlankQuizCommand>
{
    public CreateBlankQuizCommandValidator()
    {
        RuleFor(command => command.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Quiz name is required.");

        RuleFor(command => command.ResourceId)
            .Must(resourceId => !resourceId.HasValue || resourceId.Value != Guid.Empty)
            .WithMessage("ResourceId is required.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
