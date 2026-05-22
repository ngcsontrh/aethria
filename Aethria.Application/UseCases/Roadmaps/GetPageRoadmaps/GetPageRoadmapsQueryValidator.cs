namespace Aethria.Application.UseCases.Roadmaps.GetPageRoadmaps;

internal sealed class GetPageRoadmapsQueryValidator : AbstractValidator<GetPageRoadmapsQuery>
{
    public GetPageRoadmapsQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Page size must be between 1 and 50.");
    }
}
