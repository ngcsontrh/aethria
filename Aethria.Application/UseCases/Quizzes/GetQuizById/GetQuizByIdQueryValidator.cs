namespace Aethria.Application.UseCases.Quizzes.GetQuizById;

internal sealed class GetQuizByIdQueryValidator : AbstractValidator<GetQuizByIdQuery>
{
    public GetQuizByIdQueryValidator()
    {
        RuleFor(query => query.QuizId)
            .NotEmpty()
            .WithMessage("QuizId is required.");

        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
