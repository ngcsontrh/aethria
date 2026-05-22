namespace Aethria.Application.UseCases.Roadmaps.DeleteRoadmap;

internal sealed class DeleteRoadmapCommandValidator : AbstractValidator<DeleteRoadmapCommand>
{
    public DeleteRoadmapCommandValidator()
    {
        RuleFor(command => command.RoadmapId)
            .NotEmpty()
            .WithMessage("RoadmapId is required.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
