namespace Aethria.Application.UseCases.Mentors.GetMentorById;

internal sealed class GetMentorByIdQueryValidator : AbstractValidator<GetMentorByIdQuery>
{
    public GetMentorByIdQueryValidator()
    {
        RuleFor(query => query.MentorId)
            .NotEmpty()
            .WithMessage("MentorId is required.");

        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
