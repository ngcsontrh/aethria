namespace Aethria.Application.UseCases.ChatSessions.GetChatHistory;

internal sealed class GetChatHistoryQueryValidator : AbstractValidator<GetChatHistoryQuery>
{
    public GetChatHistoryQueryValidator()
    {
        RuleFor(query => query.SessionId)
            .NotEmpty()
            .WithMessage("SessionId is required.");

        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(query => query.Scope)
            .IsInEnum()
            .WithMessage("Scope is invalid.");

        When(query => query.Scope == ChatSessionScope.General, () =>
        {
            RuleFor(query => query.ScopeId)
                .Must(scopeId => scopeId is null)
                .WithMessage("ScopeId must be empty for general chat sessions.");
        });

        When(query => query.Scope is ChatSessionScope.Mentor or ChatSessionScope.Resource, () =>
        {
            RuleFor(query => query.ScopeId)
                .Must(scopeId => scopeId is not null && scopeId != Guid.Empty)
                .WithMessage("ScopeId is required for scoped chat sessions.");
        });
    }
}
