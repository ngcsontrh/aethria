namespace Aethria.Application.UseCases.ChatSessions.GetChatSessions;

internal sealed class GetChatSessionsQueryValidator : AbstractValidator<GetChatSessionsQuery>
{
    public GetChatSessionsQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Page size must be between 1 and 50.");

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
