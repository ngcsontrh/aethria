namespace Aethria.Application.UseCases.Quizzes.GetQuizQuestionsForEdit;

internal sealed class GetQuizQuestionsForEditQueryValidator : AbstractValidator<GetQuizQuestionsForEditQuery>
{
    public GetQuizQuestionsForEditQueryValidator()
    {
        RuleFor(query => query.QuizId)
            .NotEmpty()
            .WithMessage("QuizId is required.");

        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
