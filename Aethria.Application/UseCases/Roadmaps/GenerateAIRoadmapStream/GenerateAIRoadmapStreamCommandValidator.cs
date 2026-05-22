namespace Aethria.Application.UseCases.Roadmaps.GenerateAIRoadmapStream;

internal sealed class GenerateAIRoadmapStreamCommandValidator : AbstractValidator<GenerateAIRoadmapStreamCommand>
{
    public GenerateAIRoadmapStreamCommandValidator()
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
