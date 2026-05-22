namespace Aethria.Application.UseCases.Quizzes.GetQuizSubmissionHistory;

internal sealed class GetQuizSubmissionHistoryQueryValidator : AbstractValidator<GetQuizSubmissionHistoryQuery>
{
    public GetQuizSubmissionHistoryQueryValidator()
    {
        RuleFor(query => query.QuizId)
            .NotEmpty()
            .WithMessage("QuizId is required.");

        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
    }
}
