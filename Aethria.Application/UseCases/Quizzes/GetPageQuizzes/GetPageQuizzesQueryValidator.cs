namespace Aethria.Application.UseCases.Quizzes.GetPageQuizzes;

internal sealed class GetPageQuizzesQueryValidator : AbstractValidator<GetPageQuizzesQuery>
{
    public GetPageQuizzesQueryValidator()
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
