namespace Aethria.Application.UseCases.ChatSessions.DeleteAllUserChatSessions;

internal sealed class DeleteAllUserChatSessionsCommandValidator : AbstractValidator<DeleteAllUserChatSessionsCommand>
{
    public DeleteAllUserChatSessionsCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
