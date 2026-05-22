namespace Aethria.Application.UseCases.Resources.UpdateResource;

internal sealed class UpdateResourceCommandValidator : AbstractValidator<UpdateResourceCommand>
{
    public UpdateResourceCommandValidator()
    {
        RuleFor(command => command.ResourceId)
            .NotEmpty()
            .WithMessage("ResourceId is required.");

        RuleFor(command => command.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 200)
            .WithMessage("Name must be between 1 and 200 characters.");

        RuleFor(command => command.Description)
            .MaximumLength(2000)
            .WithMessage("Description must be less than 2000 characters.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
