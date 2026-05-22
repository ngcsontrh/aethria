namespace Aethria.Application.UseCases.Chat.GeneralChat;

internal sealed class GeneralChatCommandValidator : AbstractValidator<GeneralChatCommand>
{
    public GeneralChatCommandValidator()
    {
        RuleFor(command => command.Message)
            .Must(message => !string.IsNullOrWhiteSpace(message))
            .WithMessage("Message cannot be empty.")
            .Must(message => string.IsNullOrWhiteSpace(message) || message.Trim().Length <= 4000)
            .WithMessage("Message cannot exceed 4000 characters.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
