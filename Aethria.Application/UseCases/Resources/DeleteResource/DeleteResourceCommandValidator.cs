namespace Aethria.Application.UseCases.Resources.DeleteResource;

internal sealed class DeleteResourceCommandValidator : AbstractValidator<DeleteResourceCommand>
{
    public DeleteResourceCommandValidator()
    {
        RuleFor(command => command.ResourceId)
            .NotEmpty()
            .WithMessage("ResourceId is required.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
