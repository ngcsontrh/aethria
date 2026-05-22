namespace Aethria.Application.UseCases.Roadmaps.GetRoadmapById;

internal sealed class GetRoadmapByIdQueryValidator : AbstractValidator<GetRoadmapByIdQuery>
{
    public GetRoadmapByIdQueryValidator()
    {
        RuleFor(query => query.RoadmapId)
            .NotEmpty()
            .WithMessage("RoadmapId is required.");

        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
