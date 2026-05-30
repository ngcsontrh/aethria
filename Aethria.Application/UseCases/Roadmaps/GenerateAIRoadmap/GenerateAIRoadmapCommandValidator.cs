namespace Aethria.Application.UseCases.Roadmaps.GenerateAIRoadmap;

internal sealed class GenerateAIRoadmapCommandValidator : AbstractValidator<GenerateAIRoadmapCommand>
{
    public GenerateAIRoadmapCommandValidator()
    {
        RuleFor(command => command.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Roadmap name is required.");

        RuleFor(command => command.ResourceId)
            .NotEmpty()
            .WithMessage("ResourceId is required for AI roadmap generation.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
