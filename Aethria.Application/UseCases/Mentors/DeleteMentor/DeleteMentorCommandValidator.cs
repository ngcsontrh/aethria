namespace Aethria.Application.UseCases.Mentors.DeleteMentor;

internal sealed class DeleteMentorCommandValidator : AbstractValidator<DeleteMentorCommand>
{
    public DeleteMentorCommandValidator()
    {
        RuleFor(command => command.MentorId)
            .NotEmpty()
            .WithMessage("MentorId is required.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
