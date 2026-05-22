namespace Aethria.Application.UseCases.Quizzes.GetSubmissionById;

internal sealed class GetSubmissionByIdQueryValidator : AbstractValidator<GetSubmissionByIdQuery>
{
    public GetSubmissionByIdQueryValidator()
    {
        RuleFor(query => query.QuizId)
            .NotEmpty()
            .WithMessage("QuizId is required.");

        RuleFor(query => query.SubmissionId)
            .NotEmpty()
            .WithMessage("SubmissionId is required.");

        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
