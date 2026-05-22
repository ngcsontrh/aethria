namespace Aethria.Application.UseCases.ChatSessions.DeleteAllChatSessions;

internal sealed class DeleteAllChatSessionsCommandValidator : AbstractValidator<DeleteAllChatSessionsCommand>
{
    public DeleteAllChatSessionsCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(command => command.Scope)
            .IsInEnum()
            .WithMessage("Scope is invalid.");

        When(command => command.Scope == ChatSessionScope.General, () =>
        {
            RuleFor(command => command.ScopeId)
                .Must(scopeId => scopeId is null)
                .WithMessage("ScopeId must be empty for general chat sessions.");
        });

        When(command => command.Scope is ChatSessionScope.Mentor or ChatSessionScope.Resource, () =>
        {
            RuleFor(command => command.ScopeId)
                .Must(scopeId => scopeId is not null && scopeId != Guid.Empty)
                .WithMessage("ScopeId is required for scoped chat sessions.");
        });
    }
}
