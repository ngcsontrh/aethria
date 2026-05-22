namespace Aethria.Application.UseCases.Chat.ResourceChat;

internal sealed class ResourceChatCommandValidator : AbstractValidator<ResourceChatCommand>
{
    public ResourceChatCommandValidator()
    {
        RuleFor(command => command.ResourceId)
            .NotEmpty()
            .WithMessage("ResourceId is required.");

        RuleFor(command => command.Message)
            .Must(message => !string.IsNullOrWhiteSpace(message))
            .WithMessage("Message cannot be empty.")
            .Must(message => string.IsNullOrWhiteSpace(message) || message.Trim().Length <= 2000)
            .WithMessage("Message cannot exceed 2000 characters.");

        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
