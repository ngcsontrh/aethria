namespace Aethria.Application.UseCases.Quizzes.GetQuizQuestions;

internal sealed class GetQuizQuestionsQueryValidator : AbstractValidator<GetQuizQuestionsQuery>
{
    public GetQuizQuestionsQueryValidator()
    {
        RuleFor(query => query.QuizId)
            .NotEmpty()
            .WithMessage("QuizId is required.");

        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
