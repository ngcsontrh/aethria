namespace Aethria.Application.UseCases.Chat.MentorChat;

internal sealed class MentorChatCommandValidator : AbstractValidator<MentorChatCommand>
{
    public MentorChatCommandValidator()
    {
        RuleFor(command => command.Message)
            .Must(message => !string.IsNullOrWhiteSpace(message))
            .WithMessage("Message cannot be empty.")
            .Must(message => string.IsNullOrWhiteSpace(message) || message.Trim().Length <= 4000)
            .WithMessage("Message cannot exceed 4000 characters.");

        RuleFor(command => command.MentorId)
            .NotEmpty()
            .WithMessage("MentorId is required.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
